using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib.Scaffolding.Content;
using Valencina.ValencinaCode.Compat;

namespace Valencina.ValencinaCode.Monsters;

internal static class Act4EliteHelpers
{
	public static async Task ScaleForMultiplayer(Creature creature, int baseHp)
	{
		ICombatState combatState = creature.CombatState;
		int num = Math.Max(1, (combatState == null) ? 1 : combatState.Players.Count());
		if (num > 1)
		{
			await CreatureCmd.SetMaxAndCurrentHp(creature, (decimal)(baseHp * num));
		}
	}

	public static IEnumerable<Creature> LivingPlayers(ICombatState? combatState)
	{
		IEnumerable<Creature> enumerable = ((combatState != null) ? (from player in combatState.Players
			select player.Creature into creature
			where creature.IsAlive
			select creature).OrderBy(StableCreatureKey) : null);
		return enumerable ?? Enumerable.Empty<Creature>();
	}

	public static IEnumerable<Creature> LivingEnemyAllies(Creature creature)
	{
		ICombatState combatState = creature.CombatState;
		IEnumerable<Creature> enumerable = ((combatState != null) ? (from ally in ((IEnumerable<Creature>)combatState.Enemies).Select((Func<Creature, Creature>)delegate(Creature enemy)
			{
				if (enemy == null)
				{
					return (Creature)null;
				}
				MonsterModel monster = enemy.Monster;
				return (monster == null) ? null : monster.Creature;
			})
			where ally != null && ally != creature && ally.IsAlive
			select ally).Cast<Creature>().OrderBy(StableCreatureKey) : null);
		return enumerable ?? Enumerable.Empty<Creature>();
	}

	public static bool HasDeadEnemyAlly(Creature creature)
	{
		ICombatState combatState = creature.CombatState;
		if (combatState == null)
		{
			return false;
		}
		return ((IEnumerable<Creature>)combatState.Enemies).Select((Func<Creature, Creature>)delegate(Creature enemy)
		{
			if (enemy == null)
			{
				return (Creature)null;
			}
			MonsterModel monster = enemy.Monster;
			return (monster == null) ? null : monster.Creature;
		}).Any((Creature ally) => ally != null && ally != creature && !ally.IsAlive);
	}

	public static async Task CleanupRealDeath(Creature creature)
	{
		_ = 2;
		try
		{
			RemoveCreatureNode(creature);
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (val == null)
			{
				await Task.Yield();
			}
			else
			{
				await ((GodotObject)val).ToSignal((GodotObject)(object)val, SignalName.ProcessFrame);
			}
			RemoveCreatureNode(creature);
			ICombatState combatState = creature.CombatState;
			if (combatState != null && combatState.ContainsCreature(creature))
			{
				CombatManager instance = CombatManager.Instance;
				if (instance != null)
				{
					instance.RemoveCreature(creature);
				}
				combatState.RemoveCreature(creature, true);
			}
			CombatManager instance2 = CombatManager.Instance;
			if (instance2 != null && instance2.IsInProgress)
			{
				await CombatManager.Instance.CheckWinCondition();
			}
		}
		catch (Exception value)
		{
			MainFile.Logger.Warn($"[Act4Elite] Failed to clean real death state for {creature.Name}: {value}", 1);
		}
	}

	private static void RemoveCreatureNode(Creature creature)
	{
		try
		{
			NCombatRoom instance = NCombatRoom.Instance;
			NCreature val = ((instance != null) ? instance.GetCreatureNode(creature) : null);
			if (val != null && instance != null)
			{
				instance.RemoveCreatureNode(val);
			}
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[Act4Elite] Failed to remove creature node for " + creature.Name + ": " + ex.Message, 1);
		}
	}

	public static Task<AttackCommand> ExecuteMonsterAttack(ModMonsterTemplate monster, decimal damage, int hits)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		PlayEliteAttackVisual(monster);
		return DamageCmd.Attack(damage).WithHitCount(hits).FromMonster((MonsterModel)(object)monster)
			.OnlyPlayAnimOnce()
			.WithHitFx("vfx/vfx_attack_slash", (string)null, (string)null)
			.Execute((PlayerChoiceContext)new BlockingPlayerChoiceContext());
	}

	private static void PlayEliteAttackVisual(ModMonsterTemplate monster)
	{
		try
		{
			NCombatRoom instance = NCombatRoom.Instance;
			NCreature obj = ((instance != null) ? instance.GetCreatureNode(((MonsterModel)monster).Creature) : null);
			if (((obj != null) ? obj.Visuals : null) is Act4EliteCreatureVisuals act4EliteCreatureVisuals)
			{
				act4EliteCreatureVisuals.PlayAttackVisual();
			}
		}
		catch (Exception ex)
		{
			Logger logger = MainFile.Logger;
			Creature creature = ((MonsterModel)monster).Creature;
			logger.Warn("[Act4Elite] Failed to play attack visual for " + ((creature != null) ? creature.Name : null) + ": " + ex.Message, 1);
		}
	}

	public static async Task ApplyToUnblockedPlayers<TPower>(AttackCommand attack, Creature applier, decimal amount) where TPower : PowerModel
	{
		BlockingPlayerChoiceContext ctx = new BlockingPlayerChoiceContext();
		foreach (Creature item in (from result in attack.Results.SelectMany((List<DamageResult> results) => results)
			where result.Receiver.IsPlayer && result.UnblockedDamage > 0
			select result.Receiver).Distinct().OrderBy(StableCreatureKey))
		{
			await CompatPowerCmd.Apply<TPower>((PlayerChoiceContext)(object)ctx, item, amount, applier, null);
		}
	}

	internal static string StableCreatureKey(Creature creature)
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
}
