using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Systems.Duel;

namespace Valencina.ValencinaCode.Patches.Duel;

[HarmonyPatch]
internal static class DuelUnknownRoomTypePatch
{
	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(typeof(RunManager), "RollRoomTypeFor", (Type[])null, (Type[])null);
	}

	private static bool Prefix(RunManager __instance, MapPointType pointType, ref RoomType __result)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		IRunState runState = (IRunState)(object)__instance.DebugOnlyGetState();
		if ((int)pointType == 1 && DuelNodeSystem.IsDuelPoint(runState))
		{
			__result = (RoomType)6;
			return false;
		}
		return true;
	}
}
