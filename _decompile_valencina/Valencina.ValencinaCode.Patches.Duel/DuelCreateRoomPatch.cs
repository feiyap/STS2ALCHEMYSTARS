using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Events;
using Valencina.ValencinaCode.Systems.Duel;

namespace Valencina.ValencinaCode.Patches.Duel;

[HarmonyPatch]
internal static class DuelCreateRoomPatch
{
	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(typeof(RunManager), "CreateRoom", (Type[])null, (Type[])null);
	}

	private static bool Prefix(RunManager __instance, RoomType roomType, MapPointType mapPointType, ref AbstractRoom __result)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		IRunState runState = (IRunState)(object)__instance.DebugOnlyGetState();
		if ((int)roomType == 6 && (int)mapPointType == 1 && DuelNodeSystem.IsDuelPoint(runState))
		{
			__result = (AbstractRoom)new EventRoom((EventModel)(object)ModelDb.Event<DuelMemoryEvent>());
			return false;
		}
		return true;
	}
}
