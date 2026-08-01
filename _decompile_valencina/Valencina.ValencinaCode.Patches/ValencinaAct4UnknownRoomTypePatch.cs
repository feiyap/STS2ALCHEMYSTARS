using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(RunManager), "RollRoomTypeFor")]
internal static class ValencinaAct4UnknownRoomTypePatch
{
	private static bool Prefix(RunManager __instance, MapPointType pointType, ref RoomType __result)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)pointType != 1 || !UngezieferKaiserFinalBossController.TryGetRunState(__instance, out IRunState runState) || runState.CurrentActIndex < 0 || runState.CurrentActIndex >= runState.Acts.Count || !UngezieferKaiserFinalBossController.IsValencinaAct4(runState.Acts[runState.CurrentActIndex]))
		{
			return true;
		}
		__result = (RoomType)6;
		return false;
	}
}
