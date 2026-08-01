using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(ImageHelper), "GetRoomIconPath")]
internal static class UngezieferKaiserRoomIconPathPatch
{
	private static void Postfix(MapPointType mapPointType, RoomType roomType, ModelId? modelId, ref string? __result)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (UngezieferKaiserRoomIconFallback.ShouldUseKaiserIcon(mapPointType, roomType, modelId))
		{
			__result = "res://Valencina/images/ui/run_history/ungeziefer_kaiser_encounter.png";
		}
	}
}
