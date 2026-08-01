using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Scaffolding.Content;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Monsters;

public sealed class Act4EliteHeathcliff : ModMonsterTemplate
{
	private const int BaseHp = 240;

	private const int BoundKingStrengthThreshold = 10;

	private MoveState? _warning;

	private MoveState? _triple;

	private MoveState? _drawDown;

	private MoveState? _slimed;

	private MoveState? _boundKing;

	private bool _warningNext = true;

	public override int MinInitialHp => 240;

	public override int MaxInitialHp => 240;

	public override string? CustomVisualsPath => "res://Valencina/scenes/monsters/act4_elite_heathcliff.tscn";

	public override IEnumerable<string> AssetPaths => Act4EliteAssets.AllAssetPaths;

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected O, but got Unknown
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected O, but got Unknown
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Expected O, but got Unknown
		_warning = new MoveState("warning", (Func<IReadOnlyList<Creature>, Task>)WarningMove, (AbstractIntent[])(object)new AbstractIntent[1] { (AbstractIntent)new BuffIntent() });
		_triple = new MoveState("triple", (Func<IReadOnlyList<Creature>, Task>)TripleMove, (AbstractIntent[])(object)new AbstractIntent[2]
		{
			(AbstractIntent)new MultiAttackIntent(3, 3),
			(AbstractIntent)new DebuffIntent(false)
		});
		_drawDown = new MoveState("draw_down", (Func<IReadOnlyList<Creature>, Task>)DrawDownMove, (AbstractIntent[])(object)new AbstractIntent[2]
		{
			(AbstractIntent)new SingleAttackIntent(15),
			(AbstractIntent)new DebuffIntent(false)
		});
		_slimed = new MoveState("slimed", (Func<IReadOnlyList<Creature>, Task>)SlimedMove, (AbstractIntent[])(object)new AbstractIntent[1] { (AbstractIntent)new DebuffIntent(false) });
		_boundKing = new MoveState("bound_king", (Func<IReadOnlyList<Creature>, Task>)BoundKingMove, (AbstractIntent[])(object)new AbstractIntent[2]
		{
			(AbstractIntent)new SingleAttackIntent(30),
			(AbstractIntent)new DebuffIntent(false)
		});
		_warning.FollowUpState = (MonsterState)(object)_triple;
		_triple.FollowUpState = (MonsterState)(object)_warning;
		_drawDown.FollowUpState = (MonsterState)(object)_warning;
		_slimed.FollowUpState = (MonsterState)(object)_warning;
		_boundKing.FollowUpState = (MonsterState)(object)_boundKing;
		return new MonsterMoveStateMachine((IEnumerable<MonsterState>)(object)new MoveState[5] { _warning, _triple, _drawDown, _slimed, _boundKing }, (MonsterState)(object)_warning);
	}

	public override async Task AfterAddedToRoom()
	{
		await _003C_003En__0();
		await Act4EliteHelpers.ScaleForMultiplayer(((MonsterModel)this).Creature, 240);
		BlockingPlayerChoiceContext ctx = new BlockingPlayerChoiceContext();
		await CompatPowerCmd.Apply<SharedKCorpAmpoulePower>((PlayerChoiceContext)(object)ctx, ((MonsterModel)this).Creature, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
		await EnsureWarningTelegraph((PlayerChoiceContext)(object)ctx);
	}

	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (!((MonsterModel)this).Creature.IsAlive)
		{
			return;
		}
		BlockingPlayerChoiceContext ctx = new BlockingPlayerChoiceContext();
		if (((MonsterModel)this).Creature.Side != side)
		{
			if (!_warningNext)
			{
				await RemoveWarningTelegraph();
			}
			else
			{
				await EnsureWarningTelegraph((PlayerChoiceContext)(object)ctx);
			}
		}
		if (((MonsterModel)this).Creature.Side != side && HasReachedBoundKingStrength() && ((MonsterModel)this).Creature.GetPower<BoundKingPower>() == null)
		{
			await CompatPowerCmd.Apply<BoundKingPower>((PlayerChoiceContext)(object)ctx, ((MonsterModel)this).Creature, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
			if (_boundKing != null)
			{
				((MonsterModel)this).SetMoveImmediate(_boundKing, true);
			}
		}
	}

	public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		if (creature != ((MonsterModel)this).Creature || wasRemovalPrevented)
		{
			return;
		}
		foreach (Creature item in Act4EliteHelpers.LivingEnemyAllies(((MonsterModel)this).Creature))
		{
			await CompatPowerCmd.Apply<ThornsPower>(choiceContext, item, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
		}
		await Act4EliteHelpers.CleanupRealDeath(((MonsterModel)this).Creature);
	}

	private async Task WarningMove(IReadOnlyList<Creature> targets)
	{
		await CompatPowerCmd.Apply<StrengthPower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), ((MonsterModel)this).Creature, 2m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
		_warningNext = false;
		SelectNext(_warning);
	}

	private async Task TripleMove(IReadOnlyList<Creature> targets)
	{
		await Act4EliteHelpers.ApplyToUnblockedPlayers<VulnerablePower>(await Act4EliteHelpers.ExecuteMonsterAttack((ModMonsterTemplate)(object)this, 3m, 3), ((MonsterModel)this).Creature, 2m);
		_warningNext = true;
		SelectNext(_triple);
	}

	private async Task DrawDownMove(IReadOnlyList<Creature> targets)
	{
		await Act4EliteHelpers.ApplyToUnblockedPlayers<Act4EliteDrawDownNextTurnPower>(await Act4EliteHelpers.ExecuteMonsterAttack((ModMonsterTemplate)(object)this, 15m, 1), ((MonsterModel)this).Creature, 1m);
		_warningNext = true;
		SelectNext(_drawDown);
	}

	private async Task SlimedMove(IReadOnlyList<Creature> targets)
	{
		foreach (Creature player in Act4EliteHelpers.LivingPlayers(((MonsterModel)this).CombatState))
		{
			Player player2 = player.Player;
			int num;
			if (player2 == null)
			{
				num = 0;
			}
			else
			{
				PlayerCombatState playerCombatState = player2.PlayerCombatState;
				num = ((((playerCombatState != null) ? new int?(playerCombatState.DrawPile.Cards.Count) : ((int?)null)) > 0) ? 1 : 0);
			}
			if (num == 0)
			{
				await CardPileCmd.AddToCombatAndPreview<Slimed>(player, (PileType)3, 2, (Player)null, (CardPilePosition)1);
			}
			else
			{
				await CardPileCmd.AddToCombatAndPreview<Slimed>(player, (PileType)1, 2, (Player)null, (CardPilePosition)3);
			}
			await CardPileCmd.AddToCombatAndPreview<Slimed>(player, (PileType)3, 2, (Player)null, (CardPilePosition)1);
		}
		_warningNext = true;
		SelectNext(_slimed);
	}

	private async Task BoundKingMove(IReadOnlyList<Creature> targets)
	{
		AttackCommand val = await Act4EliteHelpers.ExecuteMonsterAttack((ModMonsterTemplate)(object)this, 30m, 1);
		BlockingPlayerChoiceContext ctx = new BlockingPlayerChoiceContext();
		foreach (Creature player in (from r in val.Results.SelectMany((List<DamageResult> results) => results)
			where r.Receiver.IsPlayer && r.UnblockedDamage > 0
			select r.Receiver).Distinct().OrderBy(Act4EliteHelpers.StableCreatureKey))
		{
			await CompatPowerCmd.Apply<WeakPower>((PlayerChoiceContext)(object)ctx, player, 3m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
			await CompatPowerCmd.Apply<VulnerablePower>((PlayerChoiceContext)(object)ctx, player, 3m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
			await CompatPowerCmd.Apply<FrailPower>((PlayerChoiceContext)(object)ctx, player, 3m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
		}
		_boundKing.FollowUpState = (MonsterState)(object)_boundKing;
	}

	private void SelectNext(MoveState current)
	{
		if (((MonsterModel)this).Creature.GetPower<BoundKingPower>() != null)
		{
			current.FollowUpState = (MonsterState)(object)_boundKing;
			return;
		}
		MoveState followUpState = (MoveState)((!_warningNext) ? (((MonsterModel)this).Rng.NextInt(3) switch
		{
			0 => _triple, 
			1 => _drawDown, 
			_ => _slimed, 
		}) : _warning);
		current.FollowUpState = (MonsterState)(object)followUpState;
	}

	private bool HasReachedBoundKingStrength()
	{
		StrengthPower power = ((MonsterModel)this).Creature.GetPower<StrengthPower>();
		if (power != null)
		{
			return ((PowerModel)power).Amount >= 10;
		}
		return false;
	}

	private async Task EnsureWarningTelegraph(PlayerChoiceContext ctx)
	{
		if (((MonsterModel)this).Creature.GetPower<HeathcliffWarningPower>() == null)
		{
			await CompatPowerCmd.Apply<HeathcliffWarningPower>(ctx, ((MonsterModel)this).Creature, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
		}
		if (((MonsterModel)this).Creature.GetPower<TemporaryThornsPower>() == null)
		{
			await CompatPowerCmd.Apply<ThornsPower>(ctx, ((MonsterModel)this).Creature, 5m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
			await CompatPowerCmd.Apply<TemporaryThornsPower>(ctx, ((MonsterModel)this).Creature, 5m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
		}
	}

	private async Task RemoveWarningTelegraph()
	{
		HeathcliffWarningPower power = ((MonsterModel)this).Creature.GetPower<HeathcliffWarningPower>();
		if (power != null)
		{
			await PowerCmd.Remove((PowerModel)(object)power);
		}
		TemporaryThornsPower tempThorns = ((MonsterModel)this).Creature.GetPower<TemporaryThornsPower>();
		if (tempThorns != null)
		{
			ThornsPower power2 = ((MonsterModel)this).Creature.GetPower<ThornsPower>();
			if (power2 != null)
			{
				await PowerCmd.ModifyAmount((PlayerChoiceContext)new BlockingPlayerChoiceContext(), (PowerModel)(object)power2, (decimal)(-((PowerModel)tempThorns).Amount), ((MonsterModel)this).Creature, (CardModel)null, false);
			}
			await PowerCmd.Remove((PowerModel)(object)tempThorns);
		}
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private Task _003C_003En__0()
	{
		return ((MonsterModel)this).AfterAddedToRoom();
	}
}
