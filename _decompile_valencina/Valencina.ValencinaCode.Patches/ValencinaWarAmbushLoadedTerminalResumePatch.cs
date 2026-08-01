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

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(RunManager), "ProceedFromTerminalRewardsScreen")]
internal static class ValencinaWarAmbushLoadedTerminalResumePatch
{
	private static bool Prefix(RunManager __instance, ref Task __result)
	{
		IRunState val = (IRunState)(object)__instance.DebugOnlyGetState();
		if (val != null && val.CurrentRoomCount == 1)
		{
			AbstractRoom currentRoom = val.CurrentRoom;
			CombatRoom val2 = (CombatRoom)(object)((currentRoom is CombatRoom) ? currentRoom : null);
			if (val2 != null && ValencinaWarAmbushEntryPatch.IsWarAmbushEncounter(val2.Encounter))
			{
				__result = RestoreRecordedDestinationAfterLoadedAmbush(val);
				return false;
			}
		}
		return true;
	}

	private static async Task RestoreRecordedDestinationAfterLoadedAmbush(IRunState runState)
	{
		AbstractRoom val = CreateRecordedDestination(runState);
		if (val == null)
		{
			MainFile.Logger.Warn("[SmogWarAmbush] Loaded ambush had no recorded destination after rewards; opening map instead.", 1);
			NRun instance = NRun.Instance;
			if (instance != null)
			{
				instance.GlobalUi.MapScreen.Open(false);
			}
		}
		else
		{
			ValencinaWarAmbushEntryPatch.ClearScreensForAmbushReturn(RunManager.Instance);
			await RunManager.Instance.EnterRoom(val);
		}
	}

	internal static AbstractRoom? CreateRecordedDestination(IRunState runState)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected I4, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		ModelId ambushId = ((AbstractModel)ModelDb.Encounter<WarAmbushEncounter>()).Id;
		MapPointHistoryEntry currentMapPointHistoryEntry = runState.CurrentMapPointHistoryEntry;
		MapPointRoomHistoryEntry val = ((currentMapPointHistoryEntry != null) ? ((IEnumerable<MapPointRoomHistoryEntry>)currentMapPointHistoryEntry.Rooms).FirstOrDefault((Func<MapPointRoomHistoryEntry, bool>)((MapPointRoomHistoryEntry room) => room.ModelId != ambushId)) : null);
		if (val == null)
		{
			return null;
		}
		RoomType roomType = val.RoomType;
		switch (roomType - 4)
		{
		case 2:
			if (val.ModelId != (ModelId)null)
			{
				return (AbstractRoom?)new EventRoom(ModelDb.GetById<EventModel>(val.ModelId));
			}
			break;
		case 3:
			return (AbstractRoom?)new RestSiteRoom();
		case 1:
			return (AbstractRoom?)new MerchantRoom();
		case 0:
			return (AbstractRoom?)new TreasureRoom(runState.CurrentActIndex);
		}
		return null;
	}
}
