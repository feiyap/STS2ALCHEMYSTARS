using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using Valencina.ValencinaCode.Audio;
using Valencina.ValencinaCode.Cards;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Events;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Monsters;

public sealed class UngezieferKaiser : ModMonsterTemplate
{
	private sealed class CalculatedMultiAttackIntent : AttackIntent
	{
		private readonly int _repeat;

		public override int Repeats => _repeat;

		protected override LocString IntentLabelFormat => new LocString("intents", "FORMAT_DAMAGE_MULTI");

		public CalculatedMultiAttackIntent(Func<decimal> damageCalc, int repeat)
		{
			((AttackIntent)this).DamageCalc = damageCalc;
			_repeat = repeat;
		}

		public override int GetTotalDamage(IEnumerable<Creature> targets, Creature owner)
		{
			return ((AttackIntent)this).GetSingleDamage(targets, owner) * ((AttackIntent)this).Repeats;
		}

		public override LocString GetIntentLabel(IEnumerable<Creature> targets, Creature owner)
		{
			LocString intentLabelFormat = ((AbstractIntent)this).IntentLabelFormat;
			intentLabelFormat.Add("Damage", (decimal)((AttackIntent)this).GetSingleDamage(targets, owner));
			intentLabelFormat.Add("Repeat", (decimal)((AttackIntent)this).Repeats);
			return intentLabelFormat;
		}
	}

	private sealed class TargetedSingleAttackIntent : AttackIntent
	{
		private readonly Func<Creature, decimal> _damageCalc;

		public override int Repeats => 1;

		protected override LocString IntentLabelFormat => new LocString("intents", "FORMAT_DAMAGE_SINGLE");

		public TargetedSingleAttackIntent(Func<Creature, decimal> damageCalc)
		{
			_damageCalc = damageCalc;
			((AttackIntent)this).DamageCalc = () => 0m;
		}

		public override int GetTotalDamage(IEnumerable<Creature> targets, Creature owner)
		{
			return GetTargetDamage(targets, owner);
		}

		public override LocString GetIntentLabel(IEnumerable<Creature> targets, Creature owner)
		{
			LocString intentLabelFormat = ((AbstractIntent)this).IntentLabelFormat;
			intentLabelFormat.Add("Damage", (decimal)GetTargetDamage(targets, owner));
			return intentLabelFormat;
		}

		private int GetTargetDamage(IEnumerable<Creature> targets, Creature owner)
		{
			Player me = LocalContext.GetMe(owner.CombatState);
			Creature val = ((me != null) ? me.Creature : null);
			if (val == null || !val.IsPlayer)
			{
				val = targets.FirstOrDefault((Func<Creature, bool>)((Creature t) => t.IsPlayer && t.IsAlive)) ?? targets.FirstOrDefault((Func<Creature, bool>)((Creature t) => t.IsPlayer));
			}
			if (val == null)
			{
				return 0;
			}
			decimal num = _damageCalc(val);
			if (val.Player != null)
			{
				IEnumerable<AbstractModel> enumerable = default(IEnumerable<AbstractModel>);
				num = Hook.ModifyDamage(val.Player.RunState, val.CombatState, val, owner, num, (ValueProp)8, (CardModel)null, (ModifyDamageHookType)14, (CardPreviewMode)0, ref enumerable);
			}
			return Math.Max(0, (int)num);
		}
	}

	[Flags]
	private enum KaiserNormalEffect
	{
		None = 0,
		Fist = 1,
		Whip = 2,
		March = 4,
		Rustle = 8,
		Shield = 0x10
	}

	private enum KaiserExcisionEffect
	{
		None,
		Predation,
		Dinner,
		Excision
	}

	private const int BaseHp = 800;

	private const int BasePhaseTransitionLockHp = 400;

	private const int InitialDefensePest = 20;

	private const int HardToKillAmount = 50;

	private const int CloakBlockPerDefensePest = 20;

	private const int ShieldBlock = 90;

	private const int AttackPestOnHit = 3;

	private const int BaseExcisionDamage = 2;

	private const int MaxExcisionDoubles = 12;

	private MoveState? _slash;

	private MoveState? _triple;

	private MoveState? _swarm;

	private MoveState? _heart;

	private MoveState? _excision;

	private int _turn;

	private int _citizensCycleIndex;

	private bool _skipNextDefensePestRecovery;

	private bool _heartTriggered;

	private bool _killedPlayerLastTurn;

	private bool _isHeartTelegraphed;

	private bool _isExcisionTelegraphed;

	private bool _phaseTransitionPending;

	private bool _hasTriggeredPassiveDisableChoice;

	private bool _phaseTransitionChoiceScheduled;

	private bool _isInPhaseTransitionChoice;

	private bool _disableEmperorSubjects;

	private bool _disableEmperorBlood;

	private int _strengthLossThresholdsApplied;

	private KaiserNormalEffect _normalEffect;

	private KaiserExcisionEffect _excisionEffect;

	private readonly Dictionary<Creature, int> _excisionAttackCardsPlayedByPlayer = new Dictionary<Creature, int>();

	public override int MinInitialHp => 800;

	public override int MaxInitialHp => 800;

	public override string? CustomVisualsPath => "res://Valencina/scenes/monsters/ungeziefer_kaiser.tscn";

	public override IEnumerable<string> AssetPaths => ((MonsterModel)this).AssetPaths.Concat(UngezieferKaiserAssets.AllAssetPaths);

	internal bool IsExcisionDamageWindow => _isExcisionTelegraphed;

	public bool CanLockForPhaseTransition
	{
		get
		{
			if (!_heartTriggered && !_phaseTransitionPending)
			{
				return ((MonsterModel)this).Creature.MaxHp > 0;
			}
			return false;
		}
	}

	public bool ShouldLockHpForPhaseTransition
	{
		get
		{
			if (!_heartTriggered)
			{
				return ((MonsterModel)this).Creature.MaxHp > 0;
			}
			return false;
		}
	}

	public int CurrentPhaseTransitionLockHp
	{
		get
		{
			if (((MonsterModel)this).Creature.MaxHp <= 0)
			{
				return 400;
			}
			return Math.Max(1, ((MonsterModel)this).Creature.MaxHp / 2);
		}
	}

	public bool IsPhaseTransitionChoiceActive => _isInPhaseTransitionChoice;

	public bool IsPhaseTransitionChoiceInputBlocked
	{
		get
		{
			if (!_phaseTransitionChoiceScheduled)
			{
				return _isInPhaseTransitionChoice;
			}
			return true;
		}
	}

	public bool IsEmperorSubjectsDisabled => _disableEmperorSubjects;

	public bool IsEmperorBloodDisabled => _disableEmperorBlood;

	public bool HasEnteredPhaseTwo => _heartTriggered;

	private bool IsPhaseTransitionChoicePendingOrActive
	{
		get
		{
			if (!_phaseTransitionChoiceScheduled)
			{
				return _isInPhaseTransitionChoice;
			}
			return true;
		}
	}

	private bool IsMultiplayerCombat
	{
		get
		{
			if (((MonsterModel)this).CombatState == null)
			{
				return false;
			}
			if (!((MonsterModel)this).CombatState.Players.Skip(1).Any() && ((IPlayerCollection)((MonsterModel)this).CombatState.RunState).Players.Count <= 1)
			{
				return ((MonsterModel)this).CombatState.RunState.MultiplayerScalingModel != null;
			}
			return true;
		}
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected O, but got Unknown
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Expected O, but got Unknown
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Expected O, but got Unknown
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Expected O, but got Unknown
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Expected O, but got Unknown
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Expected O, but got Unknown
		_slash = new MoveState("slash", (Func<IReadOnlyList<Creature>, Task>)SlashMove, (AbstractIntent[])(object)new AbstractIntent[1] { (AbstractIntent)new SingleAttackIntent(25) });
		_triple = new MoveState("triple", (Func<IReadOnlyList<Creature>, Task>)TripleMove, (AbstractIntent[])(object)new AbstractIntent[1] { (AbstractIntent)new MultiAttackIntent(15, 3) });
		_swarm = new MoveState("swarm", (Func<IReadOnlyList<Creature>, Task>)SwarmMove, (AbstractIntent[])(object)new AbstractIntent[1] { (AbstractIntent)new MultiAttackIntent(5, (Func<int>)(() => Math.Max(1, _turn + 2))) });
		_heart = new MoveState("heart", (Func<IReadOnlyList<Creature>, Task>)HeartMove, (AbstractIntent[])(object)new AbstractIntent[2]
		{
			(AbstractIntent)new CalculatedMultiAttackIntent(CurrentHeartIntentDamage, 4),
			(AbstractIntent)new DebuffIntent(false)
		});
		_excision = new MoveState("excision", (Func<IReadOnlyList<Creature>, Task>)ExcisionMove, (AbstractIntent[])(object)new AbstractIntent[2]
		{
			(AbstractIntent)new TargetedSingleAttackIntent(CalculateExcisionDamage),
			(AbstractIntent)new DebuffIntent(false)
		});
		_slash.FollowUpState = (MonsterState)(object)_triple;
		_triple.FollowUpState = (MonsterState)(object)_swarm;
		_swarm.FollowUpState = (MonsterState)(object)_slash;
		_heart.FollowUpState = (MonsterState)(object)_slash;
		_excision.FollowUpState = (MonsterState)(object)_slash;
		return new MonsterMoveStateMachine((IEnumerable<MonsterState>)(object)new MoveState[5] { _slash, _triple, _swarm, _heart, _excision }, (MonsterState)(object)_slash);
	}

	public override async Task AfterAddedToRoom()
	{
		await _003C_003En__0();
		BlockingPlayerChoiceContext ctx = new BlockingPlayerChoiceContext();
		await CreatureCmd.SetMaxAndCurrentHp(((MonsterModel)this).Creature, (decimal)(800 * PlayerMultiplier()));
		await ApplyPersistentDisplayPowers((PlayerChoiceContext)(object)ctx);
		await CompatPowerCmd.Apply<HardToKillPower>((PlayerChoiceContext)(object)ctx, ((MonsterModel)this).Creature, 50m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
		if (!_disableEmperorSubjects)
		{
			await CompatPowerCmd.Apply<DefensePestPower>((PlayerChoiceContext)(object)ctx, ((MonsterModel)this).Creature, 20m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
		}
		_normalEffect = RollNormalEffects();
		await ApplyDisplayedNormalPowers((PlayerChoiceContext)(object)ctx);
		foreach (Creature item in (from p in ((MonsterModel)this).CombatState.Players
			select p.Creature into c
			where c.IsAlive
			select c).OrderBy(StableCreatureKey))
		{
			await CompatPowerCmd.Apply<KCorpAmpoulePower>((PlayerChoiceContext)(object)ctx, item, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
		}
	}

	public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		if (creature.IsPlayer && !wasRemovalPrevented)
		{
			_killedPlayerLastTurn = true;
		}
		if (creature == ((MonsterModel)this).Creature && !wasRemovalPrevented)
		{
			await CleanupKaiserDeathStateAndEndCombat();
		}
		await _003C_003En__1(choiceContext, creature, wasRemovalPrevented, deathAnimLength);
	}

	public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
	{
		await _003C_003En__2(creature, delta);
		if (creature == ((MonsterModel)this).Creature && !(delta >= 0m))
		{
			if (IsPhaseTransitionChoicePendingOrActive)
			{
				await ClampToPhaseTransitionLockHp();
			}
			else
			{
				await TryEnterPhaseTwoFromCurrentHp((PlayerChoiceContext)new BlockingPlayerChoiceContext());
			}
		}
	}

	public override bool ShouldDie(Creature creature)
	{
		if (ShouldPreventPhaseTransitionDeath(creature))
		{
			_phaseTransitionPending = true;
			return false;
		}
		return ((AbstractModel)this).ShouldDie(creature);
	}

	public override bool ShouldDieLate(Creature creature)
	{
		if (ShouldPreventPhaseTransitionDeath(creature))
		{
			_phaseTransitionPending = true;
			return false;
		}
		return ((AbstractModel)this).ShouldDieLate(creature);
	}

	public override async Task AfterPreventingDeath(Creature creature)
	{
		await _003C_003En__3(creature);
		if (creature == ((MonsterModel)this).Creature && _phaseTransitionPending)
		{
			await TryEnterPhaseTwoFromCurrentHp((PlayerChoiceContext)new BlockingPlayerChoiceContext());
		}
	}

	public async Task ValencinaAfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (((MonsterModel)this).Creature.IsAlive)
		{
			BlockingPlayerChoiceContext ctx = new BlockingPlayerChoiceContext();
			if (((MonsterModel)this).Creature.Side == side)
			{
				await ApplyKaiserTurnStartEffects((PlayerChoiceContext)(object)ctx);
				return;
			}
			await RefreshTelegraphPowers((PlayerChoiceContext)(object)ctx);
			PlayKaiserTurnStartVoice();
		}
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (((MonsterModel)this).Creature.IsAlive && ((MonsterModel)this).Creature.Side == side && !_disableEmperorSubjects)
		{
			DefensePestPower power = ((MonsterModel)this).Creature.GetPower<DefensePestPower>();
			if (((power != null && ((PowerModel)power).Amount != 0) ? 1 : 0) <= (false ? 1 : 0))
			{
				_skipNextDefensePestRecovery = true;
				await CompatPowerCmd.Apply<VulnerablePower>(choiceContext, ((MonsterModel)this).Creature, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
			}
		}
	}

	public override async Task AfterShuffle(PlayerChoiceContext choiceContext, Player shuffler)
	{
		if (((MonsterModel)this).Creature.IsAlive)
		{
			if (((shuffler != null) ? shuffler.Creature : null) != null && shuffler.Creature.IsAlive)
			{
				await CardPileCmd.AddToCombatAndPreview<Infection>(shuffler.Creature, (PileType)3, 1, (Player)null, (CardPilePosition)1);
			}
		}
	}

	public static bool HasActivePhaseTransitionChoice(ICombatState? combatState)
	{
		if (combatState == null)
		{
			return false;
		}
		return combatState.Creatures.Any((Creature creature) => creature.Monster is UngezieferKaiser ungezieferKaiser && ungezieferKaiser.IsPhaseTransitionChoiceInputBlocked);
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		await _003C_003En__4(context, cardPlay);
		if (!_isExcisionTelegraphed || (int)cardPlay.Card.Type != 1 || !((MonsterModel)this).Creature.IsAlive)
		{
			return;
		}
		Player owner = cardPlay.Card.Owner;
		Creature val = ((owner != null) ? owner.Creature : null);
		if (val != null && val.IsPlayer)
		{
			_excisionAttackCardsPlayedByPlayer.TryGetValue(val, out var value);
			int num = value + 1;
			_excisionAttackCardsPlayedByPlayer[val] = num;
			EmperorExcisionTargetPower power = val.GetPower<EmperorExcisionTargetPower>();
			decimal num2 = ((decimal?)((power != null) ? new int?(((PowerModel)power).Amount) : ((int?)null))) ?? 0m;
			decimal num3 = (decimal)num - num2;
			if (num3 > 0m)
			{
				await CompatPowerCmd.Apply<EmperorExcisionTargetPower>(context, val, num3, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
			}
		}
	}

	public void MarkPhaseTransitionPending()
	{
		_phaseTransitionPending = true;
	}

	public bool ShouldPreventPhaseTransitionDeath(Creature creature)
	{
		if (creature != ((MonsterModel)this).Creature || ((MonsterModel)this).Creature.MaxHp <= 0)
		{
			return false;
		}
		if (IsPhaseTransitionChoicePendingOrActive)
		{
			return ((MonsterModel)this).Creature.CurrentHp <= CurrentPhaseTransitionLockHp;
		}
		if (!_heartTriggered)
		{
			return ((MonsterModel)this).Creature.CurrentHp <= CurrentPhaseTransitionLockHp;
		}
		return false;
	}

	public async Task TryEnterPhaseTwoFromCurrentHp(PlayerChoiceContext ctx)
	{
		if (_heartTriggered || _isInPhaseTransitionChoice || ((MonsterModel)this).Creature.MaxHp <= 0)
		{
			return;
		}
		int currentPhaseTransitionLockHp = CurrentPhaseTransitionLockHp;
		if (((MonsterModel)this).Creature.CurrentHp <= currentPhaseTransitionLockHp)
		{
			_phaseTransitionPending = true;
			if (((MonsterModel)this).Creature.CurrentHp < currentPhaseTransitionLockHp)
			{
				await CreatureCmd.SetCurrentHp(((MonsterModel)this).Creature, (decimal)currentPhaseTransitionLockHp);
			}
			await EnterPhaseTwo(ctx);
		}
	}

	private async Task ClampToPhaseTransitionLockHp()
	{
		if (((MonsterModel)this).Creature.MaxHp > 0)
		{
			int currentPhaseTransitionLockHp = CurrentPhaseTransitionLockHp;
			if (((MonsterModel)this).Creature.CurrentHp < currentPhaseTransitionLockHp)
			{
				await CreatureCmd.SetCurrentHp(((MonsterModel)this).Creature, (decimal)currentPhaseTransitionLockHp);
			}
		}
	}

	public async Task EnterPhaseTwo(PlayerChoiceContext ctx)
	{
		if (!_heartTriggered)
		{
			_heartTriggered = true;
			_phaseTransitionPending = false;
			await RemoveDebuffsFromKaiser();
			await ResetDefensePestCycle(ctx);
			if (_heart != null)
			{
				_slash.FollowUpState = (MonsterState)(object)_heart;
				_triple.FollowUpState = (MonsterState)(object)_heart;
				_swarm.FollowUpState = (MonsterState)(object)_heart;
				_excision.FollowUpState = (MonsterState)(object)_heart;
				_isHeartTelegraphed = true;
				_isExcisionTelegraphed = false;
				((MonsterModel)this).SetMoveImmediate(_heart, true);
			}
			await ShowPassiveDisableChoiceIfNeeded(ctx);
		}
	}

	public async Task ApplyQuarterHpStrengthLosses(PlayerChoiceContext ctx)
	{
		if (!_disableEmperorBlood && ((MonsterModel)this).Creature.MaxHp > 0)
		{
			decimal num = (((MonsterModel)this).Creature.MaxHp - ((MonsterModel)this).Creature.CurrentHp) / ((MonsterModel)this).Creature.MaxHp;
			int num2 = Math.Min(4, Math.Max(0, (int)Math.Floor(num / 0.25m)));
			int num3 = num2 - _strengthLossThresholdsApplied;
			if (num3 > 0)
			{
				_strengthLossThresholdsApplied = num2;
				await CompatPowerCmd.Apply<StrengthPower>(ctx, ((MonsterModel)this).Creature, (decimal)(-num3), ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
			}
		}
	}

	private MoveState ChooseNextMove()
	{
		if ((_turn + 1) % 3 == 0 && _excision != null)
		{
			return _excision;
		}
		if ((_killedPlayerLastTurn || (_phaseTransitionPending && !_heartTriggered)) && _heart != null)
		{
			_heartTriggered = true;
			_phaseTransitionPending = false;
			_killedPlayerLastTurn = false;
			return _heart;
		}
		return (MoveState)(((MonsterModel)this).Rng.NextInt(3) switch
		{
			0 => _slash ?? _triple ?? _swarm, 
			1 => _triple ?? _slash ?? _swarm, 
			_ => _swarm ?? _slash ?? _triple, 
		});
	}

	private void SelectNextFollowUp(MoveState current)
	{
		_turn++;
		MoveState val = (MoveState)(object)(current.FollowUpState = (MonsterState)(object)ChooseNextMove());
		_isHeartTelegraphed = val == _heart;
		_isExcisionTelegraphed = val == _excision;
		_excisionAttackCardsPlayedByPlayer.Clear();
		_normalEffect = ((!_isExcisionTelegraphed && !_isHeartTelegraphed) ? RollNormalEffects() : KaiserNormalEffect.None);
		_excisionEffect = (_isExcisionTelegraphed ? RollExcisionEffect() : KaiserExcisionEffect.None);
	}

	private async Task SlashMove(IReadOnlyList<Creature> targets)
	{
		List<Creature> players = LivingPlayers(targets).ToList();
		if (players.Count != 0)
		{
			await AttackAndInfect(25m, 1);
			await ResolveNormalEffectAfterAttack(players);
			SelectNextFollowUp(_slash);
		}
	}

	private async Task TripleMove(IReadOnlyList<Creature> targets)
	{
		List<Creature> players = LivingPlayers(targets).ToList();
		if (players.Count != 0)
		{
			await AttackAndInfect(15m, 3);
			await ResolveNormalEffectAfterAttack(players);
			SelectNextFollowUp(_triple);
		}
	}

	private async Task SwarmMove(IReadOnlyList<Creature> targets)
	{
		List<Creature> players = LivingPlayers(targets).ToList();
		if (players.Count != 0)
		{
			await AttackAndInfect(5m, Math.Max(1, _turn + 2));
			await ResolveNormalEffectAfterAttack(players);
			SelectNextFollowUp(_swarm);
		}
	}

	private async Task HeartMove(IReadOnlyList<Creature> targets)
	{
		List<Creature> players = LivingPlayers(targets).ToList();
		if (players.Count == 0)
		{
			return;
		}
		if (((MonsterModel)this).Creature.GetPower<AimForTheHeartPower>() == null)
		{
			await CompatPowerCmd.Apply<AimForTheHeartPower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), ((MonsterModel)this).Creature, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
		}
		PlayKaiserAttackAnimation(4);
		foreach (Creature item in players.Where((Creature p) => p.IsAlive))
		{
			decimal damage = Math.Ceiling((decimal)item.MaxHp * 0.30m);
			await AttackAndInfectSingleTarget(item, damage, 4, playVisual: false);
		}
		_isHeartTelegraphed = false;
		await RemoveOwnPower<AimForTheHeartPower>();
		SelectNextFollowUp(_heart);
	}

	private async Task ExcisionMove(IReadOnlyList<Creature> targets)
	{
		List<Creature> list = LivingPlayers(targets).ToList();
		if (list.Count == 0)
		{
			return;
		}
		BlockingPlayerChoiceContext ctx = new BlockingPlayerChoiceContext();
		PlayKaiserAttackAnimation(1);
		foreach (Creature player in list.Where((Creature p) => p.IsAlive))
		{
			decimal damage = CalculateExcisionDamage(player);
			if ((await MonsterAttackTarget(player, damage, 1)).Results.SelectMany((List<DamageResult> results) => results).Sum((DamageResult r) => r.UnblockedDamage) > 0)
			{
				await ResolveExcisionEffectOnHit((PlayerChoiceContext)(object)ctx, player, damage);
				await ApplyAttackPestOnHit(player);
			}
		}
		_isHeartTelegraphed = false;
		_isExcisionTelegraphed = false;
		_excisionAttackCardsPlayedByPlayer.Clear();
		await RemoveOwnPower<EmperorExcisionPower>();
		await RemoveExcisionTargetPowers();
		await EnsureHardToKill((PlayerChoiceContext)(object)ctx);
		SelectNextFollowUp(_excision);
	}

	private async Task ApplyKaiserTurnStartEffects(PlayerChoiceContext ctx)
	{
		await RemoveDisplayedNormalPowers();
		await RemovePlayerTurnLimitPowers();
		if (IsExcisionDamageWindow)
		{
			await RemoveOwnPower<HardToKillPower>();
			return;
		}
		await RecoverDefensePestAtTurnStart(ctx);
		await ApplyCloak(ctx);
	}

	private async Task RefreshTelegraphPowers(PlayerChoiceContext ctx)
	{
		if (_isHeartTelegraphed && ((MonsterModel)this).Creature.GetPower<AimForTheHeartPower>() == null)
		{
			await CompatPowerCmd.Apply<AimForTheHeartPower>(ctx, ((MonsterModel)this).Creature, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
		}
		else if (!_isHeartTelegraphed)
		{
			await RemoveOwnPower<AimForTheHeartPower>();
		}
		if (!_isExcisionTelegraphed)
		{
			await RemoveOwnPower<EmperorExcisionPower>();
			await RemoveDisplayedExcisionPowers();
			await RemoveExcisionTargetPowers();
		}
		else
		{
			await RemoveOwnPower<HardToKillPower>();
			if (((MonsterModel)this).Creature.GetPower<EmperorExcisionPower>() == null)
			{
				await CompatPowerCmd.Apply<EmperorExcisionPower>(ctx, ((MonsterModel)this).Creature, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
			}
			await ApplyDisplayedExcisionPower(ctx);
		}
		await ApplyPersistentDisplayPowers(ctx);
		await ApplyDisplayedNormalPowers(ctx);
	}

	private async Task ApplyPersistentDisplayPowers(PlayerChoiceContext ctx)
	{
		await RemoveOwnPower<KillMeKillMePower>();
		await RemoveOwnPower<KaiserCitizensPower>();
		await RemoveOwnPower<KaiserCloakPower>();
		await RemoveOwnPower<KaiserArmyPower>();
		await RemoveOwnPower<KaiserWrathPower>();
		if (((MonsterModel)this).Creature.GetPower<KaiserImperialMandatePower>() == null)
		{
			await CompatPowerCmd.Apply<KaiserImperialMandatePower>(ctx, ((MonsterModel)this).Creature, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
		}
		if (!_disableEmperorBlood && ((MonsterModel)this).Creature.GetPower<KaiserBloodPower>() == null)
		{
			await CompatPowerCmd.Apply<KaiserBloodPower>(ctx, ((MonsterModel)this).Creature, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
		}
		if (_disableEmperorBlood)
		{
			await RemoveOwnPower<KaiserBloodPower>();
		}
	}

	private async Task RecoverDefensePestAtTurnStart(PlayerChoiceContext ctx)
	{
		if (!_disableEmperorSubjects)
		{
			int num = _citizensCycleIndex switch
			{
				0 => 20, 
				1 => 15, 
				2 => 10, 
				3 => 5, 
				_ => 0, 
			};
			_citizensCycleIndex = (_citizensCycleIndex + 1) % 6;
			if (_skipNextDefensePestRecovery)
			{
				_skipNextDefensePestRecovery = false;
			}
			else if (num > 0)
			{
				await CompatPowerCmd.Apply<DefensePestPower>(ctx, ((MonsterModel)this).Creature, (decimal)num, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
			}
		}
	}

	private async Task ResetDefensePestCycle(PlayerChoiceContext ctx)
	{
		_citizensCycleIndex = 1;
		_skipNextDefensePestRecovery = false;
		if (!_disableEmperorSubjects)
		{
			await CompatPowerCmd.Apply<DefensePestPower>(ctx, ((MonsterModel)this).Creature, 20m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
		}
	}

	private async Task ApplyCloak(PlayerChoiceContext ctx)
	{
		DefensePestPower power = ((MonsterModel)this).Creature.GetPower<DefensePestPower>();
		int num = ((power != null) ? ((PowerModel)power).Amount : 0);
		if (num > 0)
		{
			await CreatureCmd.GainBlock(((MonsterModel)this).Creature, (decimal)(num * 20), (ValueProp)4, (CardPlay)null, false);
		}
		await EnsureHardToKill(ctx);
	}

	private async Task ShowPassiveDisableChoiceIfNeeded(PlayerChoiceContext ctx)
	{
		if (!_hasTriggeredPassiveDisableChoice && !_phaseTransitionChoiceScheduled && !_isInPhaseTransitionChoice && ((MonsterModel)this).Creature.IsAlive)
		{
			if (IsMultiplayerCombat)
			{
				await ResolveMultiplayerPhaseChoiceSynchronized(ctx);
			}
			else if (RunManager.Instance.ActionExecutor.CurrentlyRunningAction != null)
			{
				_phaseTransitionChoiceScheduled = true;
				RunManager.Instance.ActionExecutor.AfterActionExecuted += ShowPassiveDisableChoiceAfterAction;
			}
			else
			{
				await ShowPassiveDisableChoiceNow(ctx);
			}
		}
	}

	private async Task ResolveMultiplayerPhaseChoiceSynchronized(PlayerChoiceContext ctx)
	{
		_phaseTransitionChoiceScheduled = false;
		_hasTriggeredPassiveDisableChoice = true;
		_isInPhaseTransitionChoice = true;
		MainFile.Logger.Info("[UngezieferKaiser] Starting synchronized multiplayer half-HP passive choice without opening the phase-choice EventRoom.", 1);
		await ApplyPhaseChoiceInputLocks(ctx);
		try
		{
			List<Player> list = (from player in ((MonsterModel)this).CombatState.Players.Where(delegate(Player player)
				{
					Creature val3 = ((player != null) ? player.Creature : null);
					return val3 != null && val3.IsAlive;
				})
				orderby player.NetId
				select player).ToList();
			if (list.Count == 0)
			{
				await ResolvePhaseChoiceWithFallback(ctx, "no living multiplayer decision owner was available");
				return;
			}
			RunManager instance = RunManager.Instance;
			ulong? obj;
			if (instance == null)
			{
				obj = null;
			}
			else
			{
				GameAction currentlyRunningAction = instance.ActionExecutor.CurrentlyRunningAction;
				obj = ((currentlyRunningAction != null) ? new ulong?(currentlyRunningAction.OwnerId) : ((ulong?)null));
			}
			ulong? actionOwnerId = obj;
			Player decisionOwner = ((IEnumerable<Player>)list).FirstOrDefault((Func<Player, bool>)((Player player) => player.NetId == actionOwnerId)) ?? list[0];
			CardModel val = (CardModel)(object)((ICardScope)((MonsterModel)this).CombatState.RunState).CreateCard<KaiserDisableSubjectsChoice>(decisionOwner);
			CardModel val2 = (CardModel)(object)((ICardScope)((MonsterModel)this).CombatState.RunState).CreateCard<KaiserDisableBloodChoice>(decisionOwner);
			IReadOnlyList<CardModel> readOnlyList = new _003C_003Ez__ReadOnlyArray<CardModel>((CardModel[])(object)new CardModel[2] { val, val2 });
			ctx.PushModel((AbstractModel)(object)this);
			CardModel selected;
			try
			{
				selected = await CardSelectCmd.FromChooseACardScreen(ctx, readOnlyList, decisionOwner, false);
			}
			finally
			{
				ctx.PopModel((AbstractModel)(object)this);
			}
			if (!(selected is KaiserDisableBloodChoice))
			{
				await DisableEmperorSubjects(ctx);
			}
			else
			{
				await DisableEmperorBlood(ctx);
			}
			MainFile.Logger.Info($"[UngezieferKaiser] Synchronized multiplayer passive choice resolved by player {decisionOwner.NetId}: {((selected is KaiserDisableBloodChoice) ? "Emperor's Blood" : "Emperor's Subjects")} disabled.", 1);
		}
		catch (Exception value)
		{
			MainFile.Logger.Warn($"[UngezieferKaiser] Synchronized multiplayer passive choice failed; using deterministic Subjects fallback. {value}", 1);
			await ResolvePhaseChoiceWithFallback(ctx, "synchronized multiplayer choice failed");
		}
	}

	private void ShowPassiveDisableChoiceAfterAction(GameAction action)
	{
		RunManager.Instance.ActionExecutor.AfterActionExecuted -= ShowPassiveDisableChoiceAfterAction;
		if (_hasTriggeredPassiveDisableChoice || _isInPhaseTransitionChoice || !((MonsterModel)this).Creature.IsAlive)
		{
			_phaseTransitionChoiceScheduled = false;
		}
		else
		{
			TaskHelper.RunSafely(ShowPassiveDisableChoiceAfterActionDeferred());
		}
	}

	private async Task ShowPassiveDisableChoiceAfterActionDeferred()
	{
		await Task.Yield();
		RunManager instance = RunManager.Instance;
		if (((instance != null) ? instance.ActionExecutor.CurrentlyRunningAction : null) != null)
		{
			_phaseTransitionChoiceScheduled = false;
			await ShowPassiveDisableChoiceIfNeeded((PlayerChoiceContext)new BlockingPlayerChoiceContext());
		}
		else
		{
			await ShowPassiveDisableChoiceNow((PlayerChoiceContext)new BlockingPlayerChoiceContext());
		}
	}

	private async Task ShowPassiveDisableChoiceNow(PlayerChoiceContext ctx)
	{
		if (_hasTriggeredPassiveDisableChoice || _isInPhaseTransitionChoice || !((MonsterModel)this).Creature.IsAlive)
		{
			_phaseTransitionChoiceScheduled = false;
			return;
		}
		_phaseTransitionChoiceScheduled = false;
		if (!IsSafeToInsertPhaseChoiceEventRoom(out string reason))
		{
			await ResolvePhaseChoiceWithFallback(ctx, reason);
			return;
		}
		_hasTriggeredPassiveDisableChoice = true;
		_isInPhaseTransitionChoice = true;
		await ApplyPhaseChoiceInputLocks(ctx);
		try
		{
			EventRoom val = new EventRoom((EventModel)(object)ModelDb.Event<CockroachEmperorPassiveDisableEvent>());
			val.set_OnStart((Action<EventModel>)delegate(EventModel eventModel)
			{
				if (eventModel is CockroachEmperorPassiveDisableEvent cockroachEmperorPassiveDisableEvent)
				{
					cockroachEmperorPassiveDisableEvent.Configure(() => DisableEmperorSubjects(ctx), () => DisableEmperorBlood(ctx));
				}
			});
			EventRoom val2 = val;
			await RunManager.Instance.EnterRoomWithoutExitingCurrentRoom((AbstractRoom)(object)val2, false);
		}
		catch (Exception value)
		{
			MainFile.Logger.Warn($"[UngezieferKaiser] Passive disable EventRoom insertion failed; using deterministic fallback. {value}", 1);
			await ResolvePhaseChoiceWithFallback(ctx, "EventRoom insertion failed");
		}
	}

	private bool IsSafeToInsertPhaseChoiceEventRoom(out string reason)
	{
		if (IsMultiplayerCombat)
		{
			reason = "multiplayer combat is not safe for local EventRoom insertion";
			return false;
		}
		if (RunManager.Instance == null)
		{
			reason = "RunManager.Instance is unavailable";
			return false;
		}
		CombatManager instance = CombatManager.Instance;
		if (instance == null || !instance.IsInProgress)
		{
			reason = "combat is not currently in progress";
			return false;
		}
		if (((MonsterModel)this).CombatState == null)
		{
			reason = "Kaiser has no CombatState";
			return false;
		}
		if (!((MonsterModel)this).CombatState.Players.Any(delegate(Player player)
		{
			Creature val = ((player != null) ? player.Creature : null);
			return val != null && val.IsAlive;
		}))
		{
			reason = "no living player is available for phase choice";
			return false;
		}
		reason = string.Empty;
		return true;
	}

	private async Task ResolvePhaseChoiceWithFallback(PlayerChoiceContext ctx, string reason)
	{
		_phaseTransitionChoiceScheduled = false;
		_hasTriggeredPassiveDisableChoice = true;
		MainFile.Logger.Warn("[UngezieferKaiser] Skipping passive disable EventRoom and auto-disabling Emperor's Subjects: " + reason + ".", 1);
		if (_disableEmperorSubjects || _disableEmperorBlood)
		{
			await FinishPhaseTransitionChoice(ctx);
		}
		else
		{
			await DisableEmperorSubjects(ctx);
		}
	}

	public Task ResolvePhaseChoiceFallbackFromEventError(PlayerChoiceContext ctx, string reason)
	{
		return ResolvePhaseChoiceWithFallback(ctx, reason);
	}

	private async Task ApplyPhaseChoiceInputLocks(PlayerChoiceContext ctx)
	{
		if (((MonsterModel)this).CombatState == null)
		{
			return;
		}
		foreach (Creature item in (from p in ((MonsterModel)this).CombatState.Players
			select p.Creature into c
			where c.IsAlive
			select c).OrderBy(StableCreatureKey))
		{
			if (item.GetPower<KaiserPhaseChoiceInputLockPower>() == null)
			{
				await CompatPowerCmd.Apply<KaiserPhaseChoiceInputLockPower>(ctx, item, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
			}
		}
	}

	private async Task RemovePhaseChoiceInputLocks()
	{
		if (((MonsterModel)this).CombatState == null)
		{
			return;
		}
		foreach (Creature item in ((MonsterModel)this).CombatState.Players.Select((Player p) => p.Creature).OrderBy(StableCreatureKey))
		{
			KaiserPhaseChoiceInputLockPower power = item.GetPower<KaiserPhaseChoiceInputLockPower>();
			if (power != null)
			{
				await PowerCmd.Remove((PowerModel)(object)power);
			}
		}
	}

	private async Task DisableEmperorSubjects(PlayerChoiceContext ctx)
	{
		_disableEmperorSubjects = true;
		_skipNextDefensePestRecovery = false;
		await RemoveOwnPower<KaiserCitizensPower>();
		await RemoveOwnPower<DefensePestPower>();
		await ApplyPersistentDisplayPowers(ctx);
		await FinishPhaseTransitionChoice(ctx);
	}

	public Task DisableEmperorSubjectsFromPhaseChoice(PlayerChoiceContext ctx)
	{
		return DisableEmperorSubjects(ctx);
	}

	private async Task DisableEmperorBlood(PlayerChoiceContext ctx)
	{
		_disableEmperorBlood = true;
		await RemoveOwnPower<KaiserBloodPower>();
		await ApplyPersistentDisplayPowers(ctx);
		await FinishPhaseTransitionChoice(ctx);
	}

	public Task DisableEmperorBloodFromPhaseChoice(PlayerChoiceContext ctx)
	{
		return DisableEmperorBlood(ctx);
	}

	private async Task FinishPhaseTransitionChoice(PlayerChoiceContext ctx)
	{
		_isInPhaseTransitionChoice = false;
		await RemovePhaseChoiceInputLocks();
		await RefreshTelegraphPowers(ctx);
	}

	private async Task ResolveNormalEffectAfterAttack(List<Creature> players)
	{
		BlockingPlayerChoiceContext ctx = new BlockingPlayerChoiceContext();
		IEnumerable<KaiserNormalEffect> enumerable = ActiveNormalEffects();
		using IEnumerator<KaiserNormalEffect> enumerator = enumerable.GetEnumerator();
		while (enumerator.MoveNext())
		{
			switch (enumerator.Current)
			{
			case KaiserNormalEffect.Fist:
				foreach (Creature item in players.Where((Creature p) => p.IsAlive))
				{
					await ApplyRandomDebuffs((PlayerChoiceContext)(object)ctx, item, 2);
				}
				break;
			case KaiserNormalEffect.March:
				foreach (Creature item2 in players.Where((Creature p) => p.IsAlive))
				{
					await CompatPowerCmd.Apply<KaiserMarchPower>((PlayerChoiceContext)(object)ctx, item2, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
				}
				break;
			case KaiserNormalEffect.Rustle:
				foreach (Creature item3 in players.Where((Creature p) => p.IsAlive))
				{
					await CompatPowerCmd.Apply<KaiserRustlePower>((PlayerChoiceContext)(object)ctx, item3, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
				}
				break;
			}
		}
	}

	private async Task ResolveExcisionEffectOnHit(PlayerChoiceContext ctx, Creature player, decimal intentDamage)
	{
		switch (_excisionEffect)
		{
		case KaiserExcisionEffect.Predation:
		{
			int num = await RemoveDebuffsFromTarget(player);
			if (num > 0)
			{
				await MonsterAttackTarget(player, num, 1);
			}
			break;
		}
		case KaiserExcisionEffect.Dinner:
			await CreatureCmd.Heal(((MonsterModel)this).Creature, intentDamage, true);
			break;
		case KaiserExcisionEffect.Excision:
			await StealPositivePowers(ctx, player);
			break;
		}
	}

	private async Task ApplyRandomDebuffs(PlayerChoiceContext ctx, Creature player, int count)
	{
		for (int i = 0; i < count; i++)
		{
			switch (((MonsterModel)this).Rng.NextInt(3))
			{
			case 0:
				await CompatPowerCmd.Apply<WeakPower>(ctx, player, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
				break;
			case 1:
				await CompatPowerCmd.Apply<VulnerablePower>(ctx, player, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
				break;
			default:
				await CompatPowerCmd.Apply<FrailPower>(ctx, player, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
				break;
			}
		}
	}

	private KaiserNormalEffect RollNormalEffects()
	{
		int num = ((!_heartTriggered) ? 1 : 2);
		KaiserNormalEffect kaiserNormalEffect = KaiserNormalEffect.None;
		List<KaiserNormalEffect> list = new List<KaiserNormalEffect>
		{
			KaiserNormalEffect.Fist,
			KaiserNormalEffect.Whip,
			KaiserNormalEffect.March,
			KaiserNormalEffect.Rustle,
			KaiserNormalEffect.Shield
		};
		for (int i = 0; i < num; i++)
		{
			if (list.Count <= 0)
			{
				break;
			}
			int index = ((MonsterModel)this).Rng.NextInt(list.Count);
			kaiserNormalEffect |= list[index];
			list.RemoveAt(index);
		}
		return kaiserNormalEffect;
	}

	private KaiserExcisionEffect RollExcisionEffect()
	{
		return ((MonsterModel)this).Rng.NextInt(3) switch
		{
			0 => KaiserExcisionEffect.Predation, 
			1 => KaiserExcisionEffect.Dinner, 
			_ => KaiserExcisionEffect.Excision, 
		};
	}

	private IEnumerable<KaiserNormalEffect> ActiveNormalEffects()
	{
		KaiserNormalEffect[] array = new KaiserNormalEffect[5]
		{
			KaiserNormalEffect.Fist,
			KaiserNormalEffect.Whip,
			KaiserNormalEffect.March,
			KaiserNormalEffect.Rustle,
			KaiserNormalEffect.Shield
		};
		foreach (KaiserNormalEffect kaiserNormalEffect in array)
		{
			if (_normalEffect.HasFlag(kaiserNormalEffect))
			{
				yield return kaiserNormalEffect;
			}
		}
	}

	private async Task ApplyDisplayedNormalPowers(PlayerChoiceContext ctx)
	{
		await RemoveDisplayedNormalPowers();
		using IEnumerator<KaiserNormalEffect> enumerator = ActiveNormalEffects().GetEnumerator();
		while (enumerator.MoveNext())
		{
			switch (enumerator.Current)
			{
			case KaiserNormalEffect.Fist:
				await CompatPowerCmd.Apply<KaiserFistPower>(ctx, ((MonsterModel)this).Creature, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
				break;
			case KaiserNormalEffect.Whip:
				await CompatPowerCmd.Apply<KaiserWhipPower>(ctx, ((MonsterModel)this).Creature, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
				break;
			case KaiserNormalEffect.March:
				await CompatPowerCmd.Apply<KaiserMarchDisplayPower>(ctx, ((MonsterModel)this).Creature, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
				break;
			case KaiserNormalEffect.Rustle:
				await CompatPowerCmd.Apply<KaiserRustleDisplayPower>(ctx, ((MonsterModel)this).Creature, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
				break;
			case KaiserNormalEffect.Shield:
				await CompatPowerCmd.Apply<KaiserShieldPower>(ctx, ((MonsterModel)this).Creature, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
				await CreatureCmd.GainBlock(((MonsterModel)this).Creature, 90m, (ValueProp)4, (CardPlay)null, false);
				break;
			}
		}
	}

	private async Task ApplyDisplayedExcisionPower(PlayerChoiceContext ctx)
	{
		await RemoveDisplayedExcisionPowers();
		switch (_excisionEffect)
		{
		case KaiserExcisionEffect.Predation:
			await CompatPowerCmd.Apply<KaiserPredationPower>(ctx, ((MonsterModel)this).Creature, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
			break;
		case KaiserExcisionEffect.Dinner:
			await CompatPowerCmd.Apply<KaiserDinnerPower>(ctx, ((MonsterModel)this).Creature, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
			break;
		case KaiserExcisionEffect.Excision:
			await CompatPowerCmd.Apply<KaiserExcisionStealPower>(ctx, ((MonsterModel)this).Creature, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
			break;
		}
	}

	private async Task RemoveDisplayedNormalPowers()
	{
		await RemoveOwnPower<KaiserFistPower>();
		await RemoveOwnPower<KaiserWhipPower>();
		await RemoveOwnPower<KaiserMarchDisplayPower>();
		await RemoveOwnPower<KaiserRustleDisplayPower>();
		await RemoveOwnPower<KaiserShieldPower>();
	}

	private async Task RemoveDisplayedExcisionPowers()
	{
		await RemoveOwnPower<KaiserPredationPower>();
		await RemoveOwnPower<KaiserDinnerPower>();
		await RemoveOwnPower<KaiserExcisionStealPower>();
	}

	private async Task RemovePlayerTurnLimitPowers()
	{
		if (((MonsterModel)this).CombatState == null)
		{
			return;
		}
		foreach (Creature player in ((MonsterModel)this).CombatState.Players.Select((Player p) => p.Creature).OrderBy(StableCreatureKey))
		{
			KaiserMarchPower power = player.GetPower<KaiserMarchPower>();
			if (power != null)
			{
				await PowerCmd.Remove((PowerModel)(object)power);
			}
			KaiserRustlePower power2 = player.GetPower<KaiserRustlePower>();
			if (power2 != null)
			{
				await PowerCmd.Remove((PowerModel)(object)power2);
			}
		}
	}

	private decimal CalculateExcisionDamage()
	{
		return CalculateExcisionDamage(_excisionAttackCardsPlayedByPlayer.Values.DefaultIfEmpty(0).Max());
	}

	private decimal CalculateExcisionDamage(Creature player)
	{
		EmperorExcisionTargetPower power = player.GetPower<EmperorExcisionTargetPower>();
		int value;
		return CalculateExcisionDamage((power != null) ? ((PowerModel)power).Amount : (_excisionAttackCardsPlayedByPlayer.TryGetValue(player, out value) ? value : 0));
	}

	private static decimal CalculateExcisionDamage(int attackCardsPlayed)
	{
		decimal result = 2m;
		int num = Math.Min(Math.Max(0, attackCardsPlayed), 12);
		for (int i = 0; i < num; i++)
		{
			result *= 2m;
		}
		return result;
	}

	private async Task AttackAndInfect(decimal damage, int hits)
	{
		if (hits <= 0)
		{
			return;
		}
		PlayKaiserAttackAnimation(hits);
		if (_normalEffect.HasFlag(KaiserNormalEffect.Whip))
		{
			foreach (Creature item in from p in LivingPlayers(((MonsterModel)this).CombatState.Players.Select((Player p) => p.Creature).ToList())
				where p.IsAlive
				select p)
			{
				await AttackAndInfectSingleTarget(item, damage + (decimal)CountDebuffTypes(item) * 2m, hits, playVisual: false);
			}
			return;
		}
		foreach (DamageResult item2 in from r in (await DamageCmd.Attack(damage).WithHitCount(hits).FromMonster((MonsterModel)(object)this)
				.WithNoAttackerAnim()
				.Execute((PlayerChoiceContext)new BlockingPlayerChoiceContext())).Results.SelectMany((List<DamageResult> results) => results)
			where r.Receiver.IsPlayer && r.UnblockedDamage > 0
			select r)
		{
			await ApplyAttackPestOnHit(item2.Receiver);
		}
	}

	private async Task AttackAndInfectSingleTarget(Creature target, decimal damage, int hits, bool playVisual = true)
	{
		if (hits <= 0)
		{
			return;
		}
		if (playVisual)
		{
			PlayKaiserAttackAnimation(hits);
		}
		foreach (DamageResult item in from r in (await MonsterAttackTarget(target, damage, hits)).Results.SelectMany((List<DamageResult> results) => results)
			where r.Receiver == target && r.UnblockedDamage > 0
			select r)
		{
			_ = item;
			await ApplyAttackPestOnHit(target);
		}
	}

	private async Task<AttackCommand> MonsterAttackTarget(Creature target, decimal damage, int hits)
	{
		AttackCommand obj = DamageCmd.Attack(damage).WithHitCount(hits).FromMonster((MonsterModel)(object)this)
			.WithNoAttackerAnim();
		RetargetMonsterAttack(obj, target);
		return await obj.Execute((PlayerChoiceContext)new BlockingPlayerChoiceContext());
	}

	private static void RetargetMonsterAttack(AttackCommand attack, Creature target)
	{
		typeof(AttackCommand).GetField("_combatState", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(attack, null);
		attack.Targeting(target);
	}

	private async Task ApplyAttackPestOnHit(Creature target)
	{
		await CompatPowerCmd.Apply<AttackPestPower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), target, 3m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
	}

	private async Task StealPositivePowers(PlayerChoiceContext ctx, Creature target)
	{
		List<PowerModel> powers = target.Powers.Where(IsStealablePositivePower).OrderBy(StablePowerKey).ToList();
		for (int i = 0; i < 3; i++)
		{
			if (powers.Count <= 0)
			{
				break;
			}
			PowerModel power = powers[((MonsterModel)this).Rng.NextInt(powers.Count)];
			powers.Remove(power);
			int amount = Math.Max(0, power.Amount);
			await TransferStolenPositivePowerToBoss(ctx, power, amount);
			await PowerCmd.Remove(power);
		}
	}

	private static bool IsStealablePositivePower(PowerModel power)
	{
		if (power.Amount <= 0 || !power.IsVisible)
		{
			return false;
		}
		if (!(power is StrengthPower) && !(power is BreathingMethodPower))
		{
			return power is VigorPower;
		}
		return true;
	}

	private async Task TransferStolenPositivePowerToBoss(PlayerChoiceContext ctx, PowerModel power, int amount)
	{
		if (amount <= 0)
		{
			return;
		}
		if (!(power is StrengthPower))
		{
			if (!(power is BreathingMethodPower))
			{
				if (power is VigorPower)
				{
					await CompatPowerCmd.Apply<VigorPower>(ctx, ((MonsterModel)this).Creature, (decimal)amount, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
				}
			}
			else
			{
				await CompatPowerCmd.Apply<BreathingMethodPower>(ctx, ((MonsterModel)this).Creature, (decimal)amount, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
			}
		}
		else
		{
			await CompatPowerCmd.Apply<StrengthPower>(ctx, ((MonsterModel)this).Creature, (decimal)amount, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
		}
	}

	private async Task<int> RemoveDebuffsFromTarget(Creature target)
	{
		List<PowerModel> debuffs = target.Powers.Where((PowerModel power) => power.IsVisible && (int)power.Type == 2).OrderBy(StablePowerKey).ToList();
		foreach (PowerModel item in debuffs)
		{
			await PowerCmd.Remove(item);
		}
		return debuffs.Count;
	}

	private async Task RemoveDebuffsFromKaiser()
	{
		List<PowerModel> list = ((MonsterModel)this).Creature.Powers.Where((PowerModel power) => power.IsVisible && (int)power.Type == 2).OrderBy(StablePowerKey).ToList();
		foreach (PowerModel item in list)
		{
			await PowerCmd.Remove(item);
		}
	}

	private static int CountDebuffTypes(Creature target)
	{
		return target.Powers.Count((PowerModel power) => power.IsVisible && power.Amount > 0 && (int)power.Type == 2);
	}

	private void PlayKaiserAttackAnimation(int hits)
	{
		try
		{
			NCombatRoom instance = NCombatRoom.Instance;
			object obj;
			if (instance == null)
			{
				obj = null;
			}
			else
			{
				NCreature creatureNode = instance.GetCreatureNode(((MonsterModel)this).Creature);
				obj = ((creatureNode != null) ? creatureNode.Visuals : null);
			}
			if (obj is UngezieferKaiserVisuals ungezieferKaiserVisuals)
			{
				ungezieferKaiserVisuals.PlayAttack(hits);
			}
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[UngezieferKaiser] Failed to play attack animation: " + ex.GetType().Name + ": " + ex.Message, 1);
		}
	}

	private void PlayKaiserTurnStartVoice()
	{
		try
		{
			NCombatRoom instance = NCombatRoom.Instance;
			object obj;
			if (instance == null)
			{
				obj = null;
			}
			else
			{
				NCreature creatureNode = instance.GetCreatureNode(((MonsterModel)this).Creature);
				obj = ((creatureNode != null) ? creatureNode.Visuals : null);
			}
			if (obj is UngezieferKaiserVisuals ungezieferKaiserVisuals)
			{
				ungezieferKaiserVisuals.PlayTurnStartVoice();
			}
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[UngezieferKaiser] Failed to play turn-start voice: " + ex.GetType().Name + ": " + ex.Message, 1);
		}
	}

	private int PlayerMultiplier()
	{
		ICombatState combatState = ((MonsterModel)this).CombatState;
		return Math.Max(1, (combatState == null) ? 1 : combatState.Players.Count());
	}

	private static IEnumerable<Creature> LivingPlayers(IReadOnlyList<Creature> targets)
	{
		return targets.Where((Creature t) => t.IsPlayer && t.IsAlive).OrderBy(StableCreatureKey);
	}

	private decimal CurrentHeartIntentDamage()
	{
		ICombatState combatState = ((MonsterModel)this).CombatState;
		Creature val = ((combatState != null) ? (from p in combatState.Players
			select p.Creature into c
			where c.IsAlive
			select c).OrderBy(StableCreatureKey).FirstOrDefault() : null);
		if (val != null)
		{
			return Math.Ceiling((decimal)val.MaxHp * 0.30m);
		}
		return 30m;
	}

	private async Task EnsureHardToKill(PlayerChoiceContext ctx)
	{
		if (((MonsterModel)this).Creature.IsAlive)
		{
			HardToKillPower power = ((MonsterModel)this).Creature.GetPower<HardToKillPower>();
			if (power == null)
			{
				await CompatPowerCmd.Apply<HardToKillPower>(ctx, ((MonsterModel)this).Creature, 50m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
			}
			else if (((PowerModel)power).Amount < 50)
			{
				await CompatPowerCmd.Apply<HardToKillPower>(ctx, ((MonsterModel)this).Creature, (decimal)(50 - ((PowerModel)power).Amount), ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
			}
		}
	}

	private async Task CleanupKaiserDeathStateAndEndCombat()
	{
		_ = 14;
		try
		{
			ValencinaMusicManager.StopBossMusicAfterCombat(stopTransientAudioImmediately: false, restoreVanillaMusic: false);
			await RemoveOwnPower<HardToKillPower>();
			await RemoveOwnPower<KaiserBloodPower>();
			await RemoveOwnPower<KaiserCitizensPower>();
			await RemoveOwnPower<KaiserCloakPower>();
			await RemoveOwnPower<KaiserArmyPower>();
			await RemoveOwnPower<KaiserWrathPower>();
			await RemoveOwnPower<KaiserImperialMandatePower>();
			await RemoveOwnPower<DefensePestPower>();
			await RemoveOwnPower<AimForTheHeartPower>();
			await RemoveOwnPower<EmperorExcisionPower>();
			await RemoveDisplayedNormalPowers();
			await RemoveDisplayedExcisionPowers();
			await RemoveExcisionTargetPowers();
			await RemovePlayerTurnLimitPowers();
			if (((MonsterModel)this).CombatState != null && ((MonsterModel)this).CombatState.ContainsCreature(((MonsterModel)this).Creature))
			{
				CombatManager instance = CombatManager.Instance;
				if (instance != null)
				{
					instance.RemoveCreature(((MonsterModel)this).Creature);
				}
			}
			CombatManager instance2 = CombatManager.Instance;
			if (instance2 != null && instance2.IsInProgress)
			{
				await CombatManager.Instance.CheckWinCondition();
			}
		}
		catch (Exception value)
		{
			MainFile.Logger.Warn($"[UngezieferKaiser] Failed to force-clean death state: {value}", 1);
		}
	}

	private async Task RemoveOwnPower<TPower>() where TPower : PowerModel
	{
		TPower power = ((MonsterModel)this).Creature.GetPower<TPower>();
		if (power != null)
		{
			await PowerCmd.Remove((PowerModel)(object)power);
		}
	}

	private async Task RemoveExcisionTargetPowers()
	{
		if (((MonsterModel)this).CombatState == null)
		{
			return;
		}
		foreach (Creature item in ((MonsterModel)this).CombatState.Players.Select((Player p) => p.Creature).OrderBy(StableCreatureKey))
		{
			EmperorExcisionTargetPower power = item.GetPower<EmperorExcisionTargetPower>();
			if (power != null)
			{
				await PowerCmd.Remove((PowerModel)(object)power);
			}
		}
	}

	private static string StableCreatureKey(Creature creature)
	{
		object obj = creature.CombatId?.ToString("D10");
		if (obj == null)
		{
			Player player = creature.Player;
			obj = ((player != null) ? player.NetId.ToString() : null);
			if (obj == null)
			{
				MonsterModel monster = creature.Monster;
				obj = ((monster != null) ? ((AbstractModel)monster).Id.Entry : null) ?? creature.Name ?? ((object)creature).GetHashCode().ToString("D10");
			}
		}
		return (string)obj;
	}

	private static string StablePowerKey(PowerModel power)
	{
		return ((AbstractModel)power).Id.Entry + "|" + ((object)power).GetType().FullName;
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private Task _003C_003En__0()
	{
		return ((MonsterModel)this).AfterAddedToRoom();
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private Task _003C_003En__1(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		return ((AbstractModel)this).AfterDeath(choiceContext, creature, wasRemovalPrevented, deathAnimLength);
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private Task _003C_003En__2(Creature creature, decimal delta)
	{
		return ((AbstractModel)this).AfterCurrentHpChanged(creature, delta);
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private Task _003C_003En__3(Creature creature)
	{
		return ((AbstractModel)this).AfterPreventingDeath(creature);
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private Task _003C_003En__4(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		return ((AbstractModel)this).AfterCardPlayed(choiceContext, cardPlay);
	}
}
