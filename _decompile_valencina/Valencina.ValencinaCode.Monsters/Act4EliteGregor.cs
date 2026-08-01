using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Scaffolding.Content;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Monsters;

public sealed class Act4EliteGregor : ModMonsterTemplate
{
	private const int BaseHp = 200;

	private MoveState? _quad;

	private MoveState? _ten;

	private MoveState? _double;

	public override int MinInitialHp => 200;

	public override int MaxInitialHp => 200;

	public override string? CustomVisualsPath => "res://Valencina/scenes/monsters/act4_elite_gregor.tscn";

	public override IEnumerable<string> AssetPaths => Act4EliteAssets.AllAssetPaths;

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
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected O, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected O, but got Unknown
		_quad = new MoveState("quad", (Func<IReadOnlyList<Creature>, Task>)QuadMove, (AbstractIntent[])(object)new AbstractIntent[1] { (AbstractIntent)new MultiAttackIntent(4, 4) });
		_ten = new MoveState("ten", (Func<IReadOnlyList<Creature>, Task>)TenMove, (AbstractIntent[])(object)new AbstractIntent[1] { (AbstractIntent)new MultiAttackIntent(2, 10) });
		_double = new MoveState("double", (Func<IReadOnlyList<Creature>, Task>)DoubleMove, (AbstractIntent[])(object)new AbstractIntent[1] { (AbstractIntent)new MultiAttackIntent(10, 2) });
		_quad.FollowUpState = (MonsterState)(object)_ten;
		_ten.FollowUpState = (MonsterState)(object)_double;
		_double.FollowUpState = (MonsterState)(object)_quad;
		return new MonsterMoveStateMachine((IEnumerable<MonsterState>)(object)new MoveState[3] { _quad, _ten, _double }, (MonsterState)(object)_quad);
	}

	public override async Task AfterAddedToRoom()
	{
		await _003C_003En__0();
		await Act4EliteHelpers.ScaleForMultiplayer(((MonsterModel)this).Creature, 200);
		BlockingPlayerChoiceContext ctx = new BlockingPlayerChoiceContext();
		await CompatPowerCmd.Apply<SharedKCorpAmpoulePower>((PlayerChoiceContext)(object)ctx, ((MonsterModel)this).Creature, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
		await CompatPowerCmd.Apply<GregorWoundOnHitPower>((PlayerChoiceContext)(object)ctx, ((MonsterModel)this).Creature, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
		await CompatPowerCmd.Apply<GregorMercyPower>((PlayerChoiceContext)(object)ctx, ((MonsterModel)this).Creature, 1m, ((MonsterModel)this).Creature, (CardModel?)null, silent: false);
	}

	public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		if (creature != ((MonsterModel)this).Creature || wasRemovalPrevented)
		{
			return;
		}
		foreach (Creature player in Act4EliteHelpers.LivingPlayers(((MonsterModel)this).CombatState))
		{
			try
			{
				await CardPileCmd.AddToCombatAndPreview<Wound>(player, (PileType)3, 1, (Player)null, (CardPilePosition)1);
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
					await CardPileCmd.AddToCombatAndPreview<Wound>(player, (PileType)3, 1, (Player)null, (CardPilePosition)1);
				}
				else
				{
					await CardPileCmd.AddToCombatAndPreview<Wound>(player, (PileType)1, 1, (Player)null, (CardPilePosition)3);
				}
			}
			catch (Exception ex)
			{
				MainFile.Logger.Warn("[Act4EliteGregor] AfterDeath: failed to add Wound for " + player.Name + ": " + ex.Message, 1);
			}
		}
		await Act4EliteHelpers.CleanupRealDeath(((MonsterModel)this).Creature);
	}

	private async Task QuadMove(IReadOnlyList<Creature> targets)
	{
		await Act4EliteHelpers.ExecuteMonsterAttack((ModMonsterTemplate)(object)this, 4m, 4);
		SelectNext(_quad);
	}

	private async Task TenMove(IReadOnlyList<Creature> targets)
	{
		await Act4EliteHelpers.ExecuteMonsterAttack((ModMonsterTemplate)(object)this, 2m, 10);
		SelectNext(_ten);
	}

	private async Task DoubleMove(IReadOnlyList<Creature> targets)
	{
		await Act4EliteHelpers.ExecuteMonsterAttack((ModMonsterTemplate)(object)this, 10m, 2);
		SelectNext(_double);
	}

	private void SelectNext(MoveState current)
	{
		current.FollowUpState = (MonsterState)(((MonsterModel)this).Rng.NextInt(3) switch
		{
			0 => _quad, 
			1 => _ten, 
			_ => _double, 
		});
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private Task _003C_003En__0()
	{
		return ((MonsterModel)this).AfterAddedToRoom();
	}
}
