using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Scaffolding.Content;

namespace Valencina.ValencinaCode.Monsters;

public sealed class GCompanySoldierOne : GCompanySoldier
{
	private MoveState? _attack;

	private MoveState? _tripleAttack;

	private MoveState? _weak;

	public override int MinInitialHp => 45;

	public override int MaxInitialHp => 45;

	public override string? CustomVisualsPath => "res://Valencina/scenes/monsters/g_company_soldier_1.tscn";

	public override IEnumerable<string> AssetPaths => GCompanyAmbushAssets.SoldierOneAssetPaths;

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Expected O, but got Unknown
		_attack = new MoveState("attack", (Func<IReadOnlyList<Creature>, Task>)AttackMove, (AbstractIntent[])(object)new AbstractIntent[1] { (AbstractIntent)new SingleAttackIntent(10) });
		_tripleAttack = new MoveState("triple_attack", (Func<IReadOnlyList<Creature>, Task>)TripleAttackMove, (AbstractIntent[])(object)new AbstractIntent[1] { (AbstractIntent)new MultiAttackIntent(4, 3) });
		_weak = new MoveState("weak", (Func<IReadOnlyList<Creature>, Task>)WeakMove, (AbstractIntent[])(object)new AbstractIntent[1] { (AbstractIntent)new DebuffIntent(false) });
		_attack.FollowUpState = (MonsterState)(object)_tripleAttack;
		_tripleAttack.FollowUpState = (MonsterState)(object)_weak;
		_weak.FollowUpState = (MonsterState)(object)_attack;
		return new MonsterMoveStateMachine((IEnumerable<MonsterState>)new _003C_003Ez__ReadOnlyArray<MonsterState>((MonsterState[])(object)new MonsterState[3]
		{
			(MonsterState)_attack,
			(MonsterState)_tripleAttack,
			(MonsterState)_weak
		}), (MonsterState)(object)_attack);
	}

	private Task AttackMove(IReadOnlyList<Creature> targets)
	{
		return GCompanyAmbushHelpers.Attack((ModMonsterTemplate)(object)this, 10m, 1);
	}

	private Task TripleAttackMove(IReadOnlyList<Creature> targets)
	{
		return GCompanyAmbushHelpers.Attack((ModMonsterTemplate)(object)this, 4m, 3);
	}

	private Task WeakMove(IReadOnlyList<Creature> targets)
	{
		return GCompanyAmbushHelpers.ApplyToPlayers<WeakPower>(((MonsterModel)this).CombatState, ((MonsterModel)this).Creature, 2m);
	}
}
