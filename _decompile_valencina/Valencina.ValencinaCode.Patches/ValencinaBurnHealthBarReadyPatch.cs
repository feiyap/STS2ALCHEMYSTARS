using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NHealthBar), "_Ready")]
internal static class ValencinaBurnHealthBarReadyPatch
{
	private static void Postfix(NHealthBar __instance)
	{
		BurnHealthBarOverlay.Ensure(__instance);
	}
}
