using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NNormalMapPoint), "RefreshState")]
internal static class MagicBeeperQuestMarkerRefreshStatePatch
{
	private static void Postfix(NNormalMapPoint __instance)
	{
		MagicBeeperQuestMarker.Apply(__instance);
	}
}
