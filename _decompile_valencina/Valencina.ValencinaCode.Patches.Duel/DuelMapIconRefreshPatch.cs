using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace Valencina.ValencinaCode.Patches.Duel;

[HarmonyPatch(typeof(NNormalMapPoint), "RefreshState")]
internal static class DuelMapIconRefreshPatch
{
	private static void Postfix(NNormalMapPoint __instance)
	{
		DuelMapIcon.Apply(__instance);
	}
}
