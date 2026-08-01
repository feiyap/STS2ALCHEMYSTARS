using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NHealthBar), "RefreshForeground")]
internal static class ValencinaBurnHealthBarRefreshForegroundPatch
{
	private static void Postfix(NHealthBar __instance)
	{
		BurnHealthBarOverlay.Refresh(__instance);
	}
}
