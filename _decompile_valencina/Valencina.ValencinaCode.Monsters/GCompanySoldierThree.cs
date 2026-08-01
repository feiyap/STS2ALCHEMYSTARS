using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Scaffolding.Content;

namespace Valencina.ValencinaCode.Monsters;

public sealed class GCompanySoldierThree : GCompanySoldier
{
	private MoveState? _attack;

	private MoveState? _fiveHitAttack;

	private MoveState? _slimed;

	public override int MinInitialHp => 48;

	public override int MaxInitialHp => 48;

	public override string? CustomVisualsPath => "res://Valencina/scenes/monsters/g_company_soldier_3.tscn";

	public override IEnumerable<string> AssetPaths => GCompanyAmbushAssets.SoldierThreeAssetPaths;

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Expected O, but got Unknown
		_attack = new MoveState("attack", (Func<IReadOnlyList<Creature>, Task>)AttackMove, (AbstractIntent[])(object)new AbstractIntent[1] { (AbstractIntent)new SingleAttackIntent(8) });
		_fiveHitAttack = new MoveState("five_hit_attack", (Func<IReadOnlyList<Creature>, Task>)FiveHitAttackMove, (AbstractIntent[])(object)new AbstractIntent[1] { (AbstractIntent)new MultiAttackIntent(1, 5) });
		_slimed = new MoveState("slimed", (Func<IReadOnlyList<Creature>, Task>)SlimedMove, (AbstractIntent[])(object)new AbstractIntent[1] { (AbstractIntent)new DebuffIntent(false) });
		_attack.FollowUpState = (MonsterState)(object)_fiveHitAttack;
		_fiveHitAttack.FollowUpState = (MonsterState)(object)_slimed;
		_slimed.FollowUpState = (MonsterState)(object)_attack;
		return new MonsterMoveStateMachine((IEnumerable<MonsterState>)new _003C_003Ez__ReadOnlyArray<MonsterState>((MonsterState[])(object)new MonsterState[3]
		{
			(MonsterState)_attack,
			(MonsterState)_fiveHitAttack,
			(MonsterState)_slimed
		}), (MonsterState)(object)_attack);
	}

	private Task AttackMove(IReadOnlyList<Creature> targets)
	{
		return GCompanyAmbushHelpers.Attack((ModMonsterTemplate)(object)this, 8m, 1);
	}

	private Task FiveHitAttackMove(IReadOnlyList<Creature> targets)
	{
		return GCompanyAmbushHelpers.Attack((ModMonsterTemplate)(object)this, 1m, 5);
	}

	private async Task SlimedMove(IReadOnlyList<Creature> targets)
	{
		foreach (Creature item in Act4EliteHelpers.LivingPlayers(((MonsterModel)this).CombatState))
		{
			await CardPileCmd.AddToCombatAndPreview<Slimed>(item, (PileType)3, 2, (Player)null, (CardPilePosition)1);
		}
	}
}
