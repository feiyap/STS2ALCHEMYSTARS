using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(RoomSet), "FromSave")]
internal static class ValencinaEmptyRoomSetSaveCompatPatch
{
	private static void Prefix(SerializableRoomSet save)
	{
		bool flag = false;
		if (save.EventIds == null)
		{
			save.EventIds = new List<ModelId>();
			flag = true;
		}
		if (save.NormalEncounterIds == null)
		{
			save.NormalEncounterIds = new List<ModelId>();
			flag = true;
		}
		if (save.EliteEncounterIds == null)
		{
			save.EliteEncounterIds = new List<ModelId>();
			flag = true;
		}
		if (flag)
		{
			MainFile.Logger.Info("[RoomSetSaveCompat] Restored omitted empty encounter/event lists while loading a run.", 1);
		}
	}
}
