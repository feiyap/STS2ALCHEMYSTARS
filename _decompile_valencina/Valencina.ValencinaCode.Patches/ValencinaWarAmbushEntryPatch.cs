using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves;
using Valencina.ValencinaCode.Encounters;
using Valencina.ValencinaCode.Events;
using Valencina.ValencinaCode.Relics.Rien;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
internal static class ValencinaWarAmbushEntryPatch
{
	private const int BaseAmbushChancePercent = 20;

	private const int MaxAmbushesPerAct = 3;

	private const int GuaranteedByEligibleDestination = 3;

	private static readonly FieldInfo? EventNodeField = AccessTools.Field(typeof(EventModel), "<Node>k__BackingField");

	private static readonly MethodInfo? EnterRoomInternalMethod = AccessTools.Method(typeof(RunManager), "EnterRoomInternal", new Type[2]
	{
		typeof(AbstractRoom),
		typeof(bool)
	}, (Type[])null);

	private static readonly MethodInfo? ClearScreensMethod = AccessTools.Method(typeof(RunManager), "ClearScreens", Array.Empty<Type>(), (Type[])null);

	private static MethodBase TargetMethod()
	{
		return AccessTools.AsyncMoveNext((MethodBase)AccessTools.Method(typeof(RunManager), "EnterMapPointInternal", (Type[])null, (Type[])null));
	}

	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
	{
		MethodInfo enterRoom = AccessTools.Method(typeof(RunManager), "EnterRoom", new Type[1] { typeof(AbstractRoom) }, (Type[])null);
		MethodInfo replacement = AccessTools.Method(typeof(ValencinaWarAmbushEntryPatch), "EnterDestinationOrAmbush", (Type[])null, (Type[])null);
		FieldInfo preFinishedRoomField = AccessTools.Field(__originalMethod.DeclaringType, "preFinishedRoom");
		FieldInfo saveGameField = AccessTools.Field(__originalMethod.DeclaringType, "saveGame");
		bool canReplace = preFinishedRoomField != null && saveGameField != null;
		bool replaced = false;
		foreach (CodeInstruction instruction in instructions)
		{
			if (canReplace && CodeInstructionExtensions.Calls(instruction, enterRoom))
			{
				yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
				yield return new CodeInstruction(OpCodes.Ldfld, (object)preFinishedRoomField);
				yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
				yield return new CodeInstruction(OpCodes.Ldfld, (object)saveGameField);
				instruction.operand = replacement;
				replaced = true;
			}
			yield return instruction;
		}
		if (!replaced)
		{
			MainFile.Logger.Error("[SmogWarAmbush] Could not locate the destination EnterRoom call; early ambush entry is disabled.", 1);
		}
	}

	private static async Task EnterDestinationOrAmbush(RunManager manager, AbstractRoom destination, AbstractRoom? preFinishedRoom, bool saveGame)
	{
		IRunState val = (IRunState)(object)manager.DebugOnlyGetState();
		if (preFinishedRoom != null || !saveGame)
		{
			if (val != null)
			{
				CombatRoom val2 = (CombatRoom)(object)((destination is CombatRoom) ? destination : null);
				if (val2 != null && IsWarAmbushEncounter(val2.Encounter))
				{
					EnsureAmbushHistoryRecorded(val, val2);
				}
			}
			await manager.EnterRoom(destination);
			return;
		}
		bool flag = val == null || !ValencinaWarDifficulty.IsActive(val);
		if (!flag)
		{
			int currentActIndex = val.CurrentActIndex;
			bool flag2 = ((currentActIndex < 0 || currentActIndex >= 3) ? true : false);
			flag = flag2;
		}
		if (flag || !IsAmbushableDestination(destination))
		{
			await manager.EnterRoom(destination);
			return;
		}
		int num = 20;
		if (((IPlayerCollection)val).Players.Any((Player player) => player.GetRelic<Fly>() != null))
		{
			num += 10;
		}
		int num2 = CountAmbushesInCurrentAct(val);
		if (num2 >= 3)
		{
			await manager.EnterRoom(destination);
			return;
		}
		if ((num2 != 0 || CountEligibleDestinationsInCurrentAct(val) < 3) && val.Rng.Niche.NextInt(100) >= num)
		{
			await manager.EnterRoom(destination);
			return;
		}
		CombatRoom val3 = new CombatRoom(((EncounterModel)ModelDb.Encounter<WarAmbushEncounter>()).ToMutable(), val);
		val3.set_ShouldResumeParentEventAfterCombat(false);
		CombatRoom ambush = val3;
		if (EnterRoomInternalMethod == null)
		{
			MainFile.Logger.Error("[SmogWarAmbush] RunManager.EnterRoomInternal was unavailable; entering the destination normally.", 1);
			await manager.EnterRoom(destination);
			return;
		}
		MapPointHistoryEntry currentMapPointHistoryEntry = val.CurrentMapPointHistoryEntry;
		if (currentMapPointHistoryEntry != null)
		{
			currentMapPointHistoryEntry.Rooms.Add(new MapPointRoomHistoryEntry
			{
				RoomType = ((AbstractRoom)ambush).RoomType,
				ModelId = ((AbstractRoom)ambush).ModelId
			});
		}
		if (manager.CombatReplayWriter.IsEnabled)
		{
			manager.CombatReplayWriter.RecordInitialState(manager.ToSave((AbstractRoom)null));
		}
		await SaveManager.Instance.SaveRun((AbstractRoom)(object)ambush, false);
		await EnterRoomInternal(manager, (AbstractRoom)(object)ambush, isRestoringRoomStackBase: false);
	}

	private static void EnsureAmbushHistoryRecorded(IRunState runState, CombatRoom ambush)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		MapPointHistoryEntry currentMapPointHistoryEntry = runState.CurrentMapPointHistoryEntry;
		if (currentMapPointHistoryEntry != null)
		{
			ModelId ambushId = ((AbstractModel)ModelDb.Encounter<WarAmbushEncounter>()).Id;
			if (!currentMapPointHistoryEntry.Rooms.Any((MapPointRoomHistoryEntry room) => room.ModelId == ambushId))
			{
				currentMapPointHistoryEntry.Rooms.Add(new MapPointRoomHistoryEntry
				{
					RoomType = ((AbstractRoom)ambush).RoomType,
					ModelId = ambushId
				});
			}
		}
	}

	private static Task EnterRoomInternal(RunManager manager, AbstractRoom room, bool isRestoringRoomStackBase)
	{
		return (Task)(EnterRoomInternalMethod?.Invoke(manager, new object[2] { room, isRestoringRoomStackBase }) ?? throw new MissingMethodException(typeof(RunManager).FullName, "EnterRoomInternal"));
	}

	internal static void ClearScreensForAmbushReturn(RunManager manager)
	{
		if (ClearScreensMethod == null)
		{
			MainFile.Logger.Warn("[SmogWarAmbush] RunManager.ClearScreens was unavailable while restoring the ambush destination.", 1);
		}
		else
		{
			ClearScreensMethod.Invoke(manager, null);
		}
	}

	private static bool IsAmbushableDestination(AbstractRoom? room)
	{
		EventRoom val = (EventRoom)(object)((room is EventRoom) ? room : null);
		if (val != null)
		{
			EventModel canonicalEvent = val.CanonicalEvent;
			if (!(canonicalEvent is AncientEventModel) && !(canonicalEvent is DuelMemoryEvent))
			{
				goto IL_003b;
			}
		}
		else if (room is RestSiteRoom || room is MerchantRoom || room is TreasureRoom)
		{
			goto IL_003b;
		}
		return false;
		IL_003b:
		return true;
	}

	internal static bool IsWarAmbushEncounter(EncounterModel? encounter)
	{
		if (!(((encounter != null) ? ((AbstractModel)encounter).Id : null) == ((AbstractModel)ModelDb.Encounter<WarAmbushEncounter>()).Id))
		{
			return encounter is WarAmbushEncounter;
		}
		return true;
	}

	internal static bool DetachCurrentEventNodeForAmbush(IRunState runState)
	{
		if (!(runState.CurrentRoom is EventRoom))
		{
			return true;
		}
		return DetachSynchronizedEventNodes();
	}

	internal static bool DetachSynchronizedEventNodes()
	{
		if (EventNodeField == null)
		{
			MainFile.Logger.Warn("[SmogWarAmbush] Event node field was unavailable; skipping event-room ambush to preserve room recovery.", 1);
			return false;
		}
		try
		{
			foreach (EventModel @event in RunManager.Instance.EventSynchronizer.Events)
			{
				if (@event.Node != null)
				{
					EventNodeField.SetValue(@event, null);
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[SmogWarAmbush] Could not detach the event UI before ambush; ambush cancelled: " + ex.Message, 1);
			return false;
		}
	}

	private static int CountAmbushesInCurrentAct(IRunState runState)
	{
		if (runState.CurrentActIndex < 0 || runState.CurrentActIndex >= runState.MapPointHistory.Count)
		{
			return 0;
		}
		ModelId ambushId = ((AbstractModel)ModelDb.Encounter<WarAmbushEncounter>()).Id;
		return runState.MapPointHistory[runState.CurrentActIndex].SelectMany((MapPointHistoryEntry entry) => entry.Rooms).Count((MapPointRoomHistoryEntry room) => room.ModelId == ambushId);
	}

	private static int CountEligibleDestinationsInCurrentAct(IRunState runState)
	{
		if (runState.CurrentActIndex < 0 || runState.CurrentActIndex >= runState.MapPointHistory.Count)
		{
			return 0;
		}
		return runState.MapPointHistory[runState.CurrentActIndex].Count(delegate(MapPointHistoryEntry entry)
		{
			MapPointRoomHistoryEntry val = entry.Rooms.FirstOrDefault();
			return val != null && IsEligibleHistoryDestination(val);
		});
	}

	private static bool IsEligibleHistoryDestination(MapPointRoomHistoryEntry room)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Invalid comparison between Unknown and I4
		RoomType roomType = room.RoomType;
		if ((roomType - 4 <= 1 || (int)roomType == 7) ? true : false)
		{
			return true;
		}
		if ((int)room.RoomType != 6 || room.ModelId == (ModelId)null)
		{
			return false;
		}
		return !(ModelDb.GetById<EventModel>(room.ModelId) is AncientEventModel);
	}
}
