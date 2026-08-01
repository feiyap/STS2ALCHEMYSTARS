using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(Hook), "AfterDeath")]
internal static class ValencinaWarInsectSpawnPatch
{
	private sealed class DeathPositionMarker(Vector2 globalPosition)
	{
		public Vector2 GlobalPosition { get; } = globalPosition;
	}

	private readonly record struct DeathSpawnFilterSnapshot(bool ShouldRemoveAfterDeath, bool IsMinion, bool HasVanillaDeathReplacementOrSpawn);

	private const int SpawnChancePercent = 10;

	private const int SpawnPoolSize = 10;

	private static readonly ConditionalWeakTable<Creature, DeathPositionMarker> DeathPositions = new ConditionalWeakTable<Creature, DeathPositionMarker>();

	private static void Prefix(ICombatState? combatState, Creature creature, out DeathSpawnFilterSnapshot __state)
	{
		__state = new DeathSpawnFilterSnapshot(combatState == null || Hook.ShouldCreatureBeRemovedFromCombatAfterDeath(combatState, creature), creature.GetPower<MinionPower>() != null, creature.GetPower<StockPower>() != null || creature.GetPower<InfestedPower>() != null || creature.GetPower<SurprisePower>() != null);
	}

	internal static void CaptureDeathPosition(NCreature creatureNode)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		Creature entity = creatureNode.Entity;
		ICombatState combatState = entity.CombatState;
		if (entity.IsEnemy && combatState != null && ValencinaWarDifficulty.IsActive(combatState.RunState))
		{
			DeathPositions.Remove(entity);
			DeathPositions.Add(entity, new DeathPositionMarker(((Control)creatureNode).GlobalPosition));
		}
	}

	private static void Postfix(IRunState runState, ICombatState? combatState, Creature creature, bool wasRemovalPrevented, DeathSpawnFilterSnapshot __state, ref Task __result)
	{
		__result = SpawnAfterDeath(__result, runState, combatState, creature, wasRemovalPrevented, __state);
	}

	private static async Task SpawnAfterDeath(Task original, IRunState runState, ICombatState? combatState, Creature creature, bool wasRemovalPrevented, DeathSpawnFilterSnapshot filterSnapshot)
	{
		await original;
		Vector2? deathPosition = ConsumeDeathPosition(creature);
		bool flag = wasRemovalPrevented || !creature.IsEnemy || combatState == null || !ValencinaWarDifficulty.IsActive(runState) || creature.GetPower<SummonedWarInsectMarkerPower>() != null || !filterSnapshot.ShouldRemoveAfterDeath || filterSnapshot.IsMinion || filterSnapshot.HasVanillaDeathReplacementOrSpawn || runState.CurrentActIndex < 0 || runState.CurrentActIndex >= 3 || CountEnteredCombats(runState) <= 3 || IsEventDrivenCombat(runState, combatState);
		bool flag2;
		if (!flag)
		{
			EncounterModel encounter = combatState.Encounter;
			RoomType? val = ((encounter != null) ? new RoomType?(encounter.RoomType) : ((RoomType?)null));
			if (val.HasValue)
			{
				RoomType valueOrDefault = val.GetValueOrDefault();
				if (valueOrDefault - 1 <= 1)
				{
					flag2 = true;
					goto IL_016b;
				}
			}
			flag2 = false;
			goto IL_016b;
		}
		goto IL_0172;
		IL_0172:
		if (!flag && combatState.IsLiveCombat() && runState.Rng.Niche.NextInt(100) < 10)
		{
			int num = runState.Rng.Niche.NextInt(10);
			string behaviorSlot = null;
			MonsterModel monster;
			switch (num)
			{
			case 0:
				monster = ((MonsterModel)ModelDb.Monster<Wriggler>()).ToMutable();
				behaviorSlot = "wriggler1";
				break;
			case 1:
				monster = ((MonsterModel)ModelDb.Monster<BowlbugEgg>()).ToMutable();
				break;
			case 2:
				monster = ((MonsterModel)ModelDb.Monster<BowlbugNectar>()).ToMutable();
				break;
			case 3:
				monster = ((MonsterModel)ModelDb.Monster<BowlbugRock>()).ToMutable();
				break;
			case 4:
				monster = ((MonsterModel)ModelDb.Monster<BowlbugSilk>()).ToMutable();
				break;
			case 5:
				monster = ((MonsterModel)ModelDb.Monster<Exoskeleton>()).ToMutable();
				behaviorSlot = "first";
				break;
			case 6:
				monster = ((MonsterModel)ModelDb.Monster<Myte>()).ToMutable();
				behaviorSlot = "first";
				break;
			case 7:
				monster = ((MonsterModel)ModelDb.Monster<ThievingHopper>()).ToMutable();
				break;
			case 8:
				monster = ((MonsterModel)ModelDb.Monster<ShrinkerBeetle>()).ToMutable();
				break;
			default:
				monster = ((MonsterModel)ModelDb.Monster<FuzzyWurmCrawler>()).ToMutable();
				break;
			}
			await AddAtDeathPosition(monster, combatState, behaviorSlot, deathPosition);
		}
		return;
		IL_016b:
		flag = !flag2;
		goto IL_0172;
	}

	private static int CountEnteredCombats(IRunState runState)
	{
		return runState.MapPointHistory.SelectMany((IReadOnlyList<MapPointHistoryEntry> actHistory) => actHistory).SelectMany((MapPointHistoryEntry mapPoint) => mapPoint.Rooms).Count(delegate(MapPointRoomHistoryEntry room)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Invalid comparison between Unknown and I4
			RoomType roomType = room.RoomType;
			return roomType - 1 <= 2;
		});
	}

	private static bool IsEventDrivenCombat(IRunState runState, ICombatState combatState)
	{
		AbstractRoom currentRoom = runState.CurrentRoom;
		CombatRoom val = (CombatRoom)(object)((currentRoom is CombatRoom) ? currentRoom : null);
		if (val != null)
		{
			if ((object)val.CombatState != combatState)
			{
				return true;
			}
			if (val.ParentEventId != (ModelId)null || !val.ShouldCreateCombat || runState.CurrentRoomCount > 1)
			{
				return true;
			}
			try
			{
				NEventRoom instance = NEventRoom.Instance;
				if (((instance != null) ? instance.EmbeddedCombatRoom : null) == NCombatRoom.Instance)
				{
					return true;
				}
			}
			catch
			{
			}
			return false;
		}
		return true;
	}

	private static Vector2? ConsumeDeathPosition(Creature creature)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (!DeathPositions.TryGetValue(creature, out DeathPositionMarker value))
		{
			return null;
		}
		DeathPositions.Remove(creature);
		return value.GlobalPosition;
	}

	private static async Task<Creature> AddAtDeathPosition(MonsterModel monster, ICombatState combatState, string? behaviorSlot, Vector2? deathPosition)
	{
		((AbstractModel)monster).AssertMutable();
		Creature spawned = combatState.CreateCreature(monster, (CombatSide)2, (string)null);
		combatState.AddCreature(spawned);
		CombatManager.Instance.AddCreature(spawned);
		NCombatRoom instance = NCombatRoom.Instance;
		if (instance != null)
		{
			instance.AddCreature(spawned);
		}
		if (deathPosition.HasValue)
		{
			NCreature val = ((instance != null) ? instance.GetCreatureNode(spawned) : null);
			if (val != null)
			{
				((Control)val).GlobalPosition = deathPosition.Value;
			}
		}
		spawned.SlotName = behaviorSlot;
		await CombatManager.Instance.AfterCreatureAdded(spawned);
		await CompatPowerCmd.Apply<SummonedWarInsectMarkerPower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), spawned, 1m, spawned, (CardModel?)null, silent: false);
		spawned.PrepareForNextTurn(combatState.Players.Select((Player player) => player.Creature), false);
		MapPointHistoryEntry currentMapPointHistoryEntry = combatState.RunState.CurrentMapPointHistoryEntry;
		MapPointRoomHistoryEntry val2 = ((currentMapPointHistoryEntry != null) ? currentMapPointHistoryEntry.Rooms.Last() : null);
		if (val2 != null && !val2.MonsterIds.Contains(((AbstractModel)monster).Id))
		{
			val2.MonsterIds.Add(((AbstractModel)monster).Id);
		}
		await Hook.AfterCreatureAddedToCombat(combatState, spawned);
		return spawned;
	}
}
