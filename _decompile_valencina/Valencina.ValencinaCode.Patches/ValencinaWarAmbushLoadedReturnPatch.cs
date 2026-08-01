using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using Valencina.ValencinaCode.Encounters;
using Valencina.ValencinaCode.Events;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(EventRoom), "Resume")]
internal static class ValencinaWarAmbushLoadedReturnPatch
{
	private static bool Prefix(EventRoom __instance, AbstractRoom exitedRoom, IRunState? runState, ref Task __result)
	{
		CombatRoom val = (CombatRoom)(object)((exitedRoom is CombatRoom) ? exitedRoom : null);
		if (val == null || !ValencinaWarAmbushEntryPatch.IsWarAmbushEncounter(val.Encounter) || runState == null)
		{
			return true;
		}
		if (!(__instance.CanonicalEvent is WarAmbushReturnEvent))
		{
			if (ValencinaWarAmbushEntryPatch.DetachSynchronizedEventNodes())
			{
				return true;
			}
			MainFile.Logger.Warn("[SmogWarAmbush] Could not repair the event UI after ambush; opening the map instead of resuming a broken event.", 1);
			__result = OpenMapSafely();
			return false;
		}
		__result = RestoreRecordedDestination(runState);
		return false;
	}

	private static Task OpenMapSafely()
	{
		NRun instance = NRun.Instance;
		if (instance != null)
		{
			instance.GlobalUi.MapScreen.Open(false);
		}
		return Task.CompletedTask;
	}

	private static async Task RestoreRecordedDestination(IRunState runState)
	{
		MapPointHistoryEntry currentMapPointHistoryEntry = runState.CurrentMapPointHistoryEntry;
		MapPointRoomHistoryEntry val = ((currentMapPointHistoryEntry != null) ? ((IEnumerable<MapPointRoomHistoryEntry>)currentMapPointHistoryEntry.Rooms).FirstOrDefault((Func<MapPointRoomHistoryEntry, bool>)((MapPointRoomHistoryEntry room) => room.ModelId != ((AbstractModel)ModelDb.Encounter<WarAmbushEncounter>()).Id)) : null);
		if (val == null)
		{
			MainFile.Logger.Warn("[SmogWarAmbush] Could not restore the destination after loading an ambush; opening the map instead.", 1);
			NRun instance = NRun.Instance;
			if (instance != null)
			{
				instance.GlobalUi.MapScreen.Open(false);
			}
			return;
		}
		RoomType roomType = val.RoomType;
		AbstractRoom val2;
		switch (roomType - 4)
		{
		case 2:
			if (val.ModelId != (ModelId)null)
			{
				val2 = (AbstractRoom)new EventRoom(ModelDb.GetById<EventModel>(val.ModelId));
				break;
			}
			goto default;
		case 3:
			val2 = (AbstractRoom)new RestSiteRoom();
			break;
		case 1:
			val2 = (AbstractRoom)new MerchantRoom();
			break;
		case 0:
			val2 = (AbstractRoom)new TreasureRoom(runState.CurrentActIndex);
			break;
		default:
			throw new InvalidOperationException($"Unsupported Smog War ambush return room: {val.RoomType}/{val.ModelId}");
		}
		AbstractRoom val3 = val2;
		ValencinaWarAmbushEntryPatch.ClearScreensForAmbushReturn(RunManager.Instance);
		await RunManager.Instance.EnterRoom(val3);
	}
}
