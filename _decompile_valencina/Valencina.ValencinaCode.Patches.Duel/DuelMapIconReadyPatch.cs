using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace Valencina.ValencinaCode.Patches.Duel;

[HarmonyPatch(typeof(NNormalMapPoint), "_Ready")]
internal static class DuelMapIconReadyPatch
{
	private static void Postfix(NNormalMapPoint __instance)
	{
		DuelMapIcon.Apply(__instance);
	}
}
