using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

namespace Valencina.ValencinaCode.Monsters;

public sealed class GCompanyMinister : ModMonsterTemplate
{
	private MoveState? _strength;

	private MoveState? _vulnerable;

	private MoveState? _frail;

	private MoveState? _punish;

	private bool _teammateDied;

	public override int MinInitialHp => 100;

	public override int MaxInitialHp => 100;

	public override string? CustomVisualsPath => "res://Valencina/scenes/monsters/g_company_minister.tscn";

	public override IEnumerable<string> AssetPaths => GCompanyAmbushAssets.MinisterAssetPaths;

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected O, but got Unknown
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Expected O, but got Unknown
		_strength = new MoveState("strength_all", (Func<IReadOnlyList<Creature>, Task>)StrengthAllMove, (AbstractIntent[])(object)new AbstractIntent[1] { (AbstractIntent)new BuffIntent() });
		_vulnerable = new MoveState("vulnerable", (Func<IReadOnlyList<Creature>, Task>)VulnerableMove, (AbstractIntent[])(object)new AbstractIntent[1] { (AbstractIntent)new DebuffIntent(false) });
		_frail = new MoveState("frail", (Func<IReadOnlyList<Creature>, Task>)FrailMove, (AbstractIntent[])(object)new AbstractIntent[1] { (AbstractIntent)new DebuffIntent(false) });
		_punish = new MoveState("punish", (Func<IReadOnlyList<Creature>, Task>)PunishMove, (AbstractIntent[])(object)new AbstractIntent[1] { (AbstractIntent)new MultiAttackIntent(7, 3) });
		_strength.FollowUpState = (MonsterState)(object)_vulnerable;
		_vulnerable.FollowUpState = (MonsterState)(object)_frail;
		_frail.FollowUpState = (MonsterState)(object)(_teammateDied ? _punish : _strength);
		_punish.FollowUpState = (MonsterState)(object)_strength;
		return new MonsterMoveStateMachine((IEnumerable<MonsterState>)new _003C_003Ez__ReadOnlyArray<MonsterState>((MonsterState[])(object)new MonsterState[4]
		{
			(MonsterState)_strength,
			(MonsterState)_vulnerable,
			(MonsterState)_frail,
			(MonsterState)_punish
		}), (MonsterState)(object)_strength);
	}

	public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (!_teammateDied && ((MonsterModel)this).Creature.IsAlive && side != ((MonsterModel)this).Creature.Side && combatState.Enemies.Count((Creature enemy) => ((enemy != null) ? enemy.Monster : null) is GCompanySoldier && enemy.Monster.Creature.IsAlive) < 2)
		{
			NotifyTeammateDeath();
		}
		return Task.CompletedTask;
	}

	public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		if (creature != ((MonsterModel)this).Creature && !wasRemovalPrevented && creature.Monster is GCompanySoldier)
		{
			NotifyTeammateDeath();
		}
		await _003C_003En__0(choiceContext, creature, wasRemovalPrevented, deathAnimLength);
	}

	internal void NotifyTeammateDeath()
	{
		_teammateDied = true;
		if (_frail != null && _punish != null)
		{
			_frail.FollowUpState = (MonsterState)(object)_punish;
		}
	}

	private async Task StrengthAllMove(IReadOnlyList<Creature> targets)
	{
		BlockingPlayerChoiceContext context = new BlockingPlayerChoiceContext();
		IEnumerable<Creature> enumerable = (from creature in ((IEnumerable<Creature>)(object)new Creature[1] { ((MonsterModel)this).Creature }).Concat(Act4EliteHelpers.LivingEnemyAllies(((MonsterModel)this).Creature))
			where creature.IsAlive
			select creature).Distinct().OrderBy(Act4EliteHelpers.StableCreatureKey);
		foreach (Creature item in enumerable)
		{
			await CompatPowerCmd.Apply<StrengthPower>((PlayerChoiceContext)(object)context, item, 3m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
		}
	}

	private Task VulnerableMove(IReadOnlyList<Creature> targets)
	{
		return GCompanyAmbushHelpers.ApplyToPlayers<VulnerablePower>(((MonsterModel)this).CombatState, ((MonsterModel)this).Creature, 1m);
	}

	private Task FrailMove(IReadOnlyList<Creature> targets)
	{
		return GCompanyAmbushHelpers.ApplyToPlayers<FrailPower>(((MonsterModel)this).CombatState, ((MonsterModel)this).Creature, 2m);
	}

	private Task PunishMove(IReadOnlyList<Creature> targets)
	{
		return GCompanyAmbushHelpers.Attack((ModMonsterTemplate)(object)this, 7m, 3);
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private Task _003C_003En__0(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		return ((AbstractModel)this).AfterDeath(choiceContext, creature, wasRemovalPrevented, deathAnimLength);
	}
}
