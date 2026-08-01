using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Scaffolding.Content;
using Valencina.ValencinaCode.Compat;

namespace Valencina.ValencinaCode.Monsters;

public sealed class GCompanySoldierTwo : GCompanySoldier
{
	private MoveState? _attack;

	private MoveState? _strength;

	public override int MinInitialHp => 50;

	public override int MaxInitialHp => 50;

	public override string? CustomVisualsPath => "res://Valencina/scenes/monsters/g_company_soldier_2.tscn";

	public override IEnumerable<string> AssetPaths => GCompanyAmbushAssets.SoldierTwoAssetPaths;

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		_attack = new MoveState("attack", (Func<IReadOnlyList<Creature>, Task>)AttackMove, (AbstractIntent[])(object)new AbstractIntent[1] { (AbstractIntent)new SingleAttackIntent(5) });
		_strength = new MoveState("strength", (Func<IReadOnlyList<Creature>, Task>)StrengthMove, (AbstractIntent[])(object)new AbstractIntent[1] { (AbstractIntent)new BuffIntent() });
		_attack.FollowUpState = (MonsterState)(object)_strength;
		_strength.FollowUpState = (MonsterState)(object)_attack;
		return new MonsterMoveStateMachine((IEnumerable<MonsterState>)new _003C_003Ez__ReadOnlyArray<MonsterState>((MonsterState[])(object)new MonsterState[2]
		{
			(MonsterState)_attack,
			(MonsterState)_strength
		}), (MonsterState)(object)_attack);
	}

	private Task AttackMove(IReadOnlyList<Creature> targets)
	{
		return GCompanyAmbushHelpers.Attack((ModMonsterTemplate)(object)this, 5m, 1);
	}

	private async Task StrengthMove(IReadOnlyList<Creature> targets)
	{
		await CompatPowerCmd.Apply<StrengthPower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), ((MonsterModel)this).Creature, 10m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
	}
}
