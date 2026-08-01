using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Scaffolding.Content;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Monsters;

public sealed class Act4EliteRodya : ModMonsterTemplate
{
	private const int BaseHp = 250;

	private MoveState? _weakSlash;

	private MoveState? _frailCombo;

	private MoveState? _heavyCombo;

	private int _turnsActed;

	public override int MinInitialHp => 250;

	public override int MaxInitialHp => 250;

	public override string? CustomVisualsPath => "res://Valencina/scenes/monsters/act4_elite_rodya.tscn";

	public override IEnumerable<string> AssetPaths => Act4EliteAssets.AllAssetPaths;

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected O, but got Unknown
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Expected O, but got Unknown
		_weakSlash = new MoveState("weak_slash", (Func<IReadOnlyList<Creature>, Task>)WeakSlashMove, (AbstractIntent[])(object)new AbstractIntent[2]
		{
			(AbstractIntent)new MultiAttackIntent(8, TearBladeAdjustedHits(2)),
			(AbstractIntent)new DebuffIntent(false)
		});
		_frailCombo = new MoveState("frail_combo", (Func<IReadOnlyList<Creature>, Task>)FrailComboMove, (AbstractIntent[])(object)new AbstractIntent[2]
		{
			(AbstractIntent)new MultiAttackIntent(4, TearBladeAdjustedHits(3)),
			(AbstractIntent)new DebuffIntent(false)
		});
		_heavyCombo = new MoveState("heavy_combo", (Func<IReadOnlyList<Creature>, Task>)HeavyComboMove, (AbstractIntent[])(object)new AbstractIntent[1] { (AbstractIntent)new MultiAttackIntent(8, TearBladeAdjustedHits(3)) });
		_weakSlash.FollowUpState = (MonsterState)(object)_heavyCombo;
		_frailCombo.FollowUpState = (MonsterState)(object)_heavyCombo;
		_heavyCombo.FollowUpState = (MonsterState)(object)_weakSlash;
		return new MonsterMoveStateMachine((IEnumerable<MonsterState>)(object)new MoveState[3] { _weakSlash, _frailCombo, _heavyCombo }, (MonsterState)(object)_weakSlash);
	}

	public override async Task AfterAddedToRoom()
	{
		await _003C_003En__0();
		SharedKCorpAmpoulePower.ResetForCombat(((MonsterModel)this).CombatState);
		await Act4EliteHelpers.ScaleForMultiplayer(((MonsterModel)this).Creature, 250);
		BlockingPlayerChoiceContext ctx = new BlockingPlayerChoiceContext();
		await CompatPowerCmd.Apply<SharedKCorpAmpoulePower>((PlayerChoiceContext)(object)ctx, ((MonsterModel)this).Creature, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
		await CompatPowerCmd.Apply<RodyaGuardPower>((PlayerChoiceContext)(object)ctx, ((MonsterModel)this).Creature, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
	}

	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (((MonsterModel)this).Creature.Side == side || !((MonsterModel)this).Creature.IsAlive || (_turnsActed < 3 && !Act4EliteHelpers.HasDeadEnemyAlly(((MonsterModel)this).Creature)))
		{
			return;
		}
		TearBladePower power = ((MonsterModel)this).Creature.GetPower<TearBladePower>();
		if (power == null || ((PowerModel)power).Amount < 3)
		{
			TearBladePower power2 = ((MonsterModel)this).Creature.GetPower<TearBladePower>();
			decimal before = ((decimal?)((power2 != null) ? new int?(((PowerModel)power2).Amount) : ((int?)null))) ?? 0m;
			await CompatPowerCmd.Apply<TearBladePower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), ((MonsterModel)this).Creature, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
			if (before < 1m)
			{
				await CompatPowerCmd.Apply<StrengthPower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), ((MonsterModel)this).Creature, 3m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
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
			await CompatPowerCmd.Apply<StrengthPower>(choiceContext, item, 3m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
		}
		await Act4EliteHelpers.CleanupRealDeath(((MonsterModel)this).Creature);
	}

	private async Task WeakSlashMove(IReadOnlyList<Creature> targets)
	{
		await Act4EliteHelpers.ApplyToUnblockedPlayers<WeakPower>(await Act4EliteHelpers.ExecuteMonsterAttack((ModMonsterTemplate)(object)this, 8m, 2), ((MonsterModel)this).Creature, 2m);
		SelectNext(_weakSlash);
	}

	private async Task FrailComboMove(IReadOnlyList<Creature> targets)
	{
		await Act4EliteHelpers.ApplyToUnblockedPlayers<FrailPower>(await Act4EliteHelpers.ExecuteMonsterAttack((ModMonsterTemplate)(object)this, 4m, 3), ((MonsterModel)this).Creature, 2m);
		SelectNext(_frailCombo);
	}

	private async Task HeavyComboMove(IReadOnlyList<Creature> targets)
	{
		await Act4EliteHelpers.ExecuteMonsterAttack((ModMonsterTemplate)(object)this, 8m, 3);
		SelectNext(_heavyCombo);
	}

	private void SelectNext(MoveState current)
	{
		_turnsActed++;
		TearBladePower power = ((MonsterModel)this).Creature.GetPower<TearBladePower>();
		MoveState val = (MoveState)((power == null || ((PowerModel)power).Amount < 3) ? (((MonsterModel)this).Rng.NextInt(3) switch
		{
			0 => _weakSlash, 
			1 => _frailCombo, 
			_ => _heavyCombo, 
		}) : _heavyCombo);
		MoveState followUpState = val;
		current.FollowUpState = (MonsterState)(object)followUpState;
	}

	private int TearBladeAdjustedHits(int baseHits)
	{
		try
		{
			TearBladePower power = ((MonsterModel)this).Creature.GetPower<TearBladePower>();
			return (power != null && ((PowerModel)power).Amount >= 3) ? (baseHits + 1) : baseHits;
		}
		catch (InvalidOperationException)
		{
			return baseHits;
		}
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private Task _003C_003En__0()
	{
		return ((MonsterModel)this).AfterAddedToRoom();
	}
}
