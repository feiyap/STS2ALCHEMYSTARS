using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NPower), "_Ready")]
internal static class ValencinaPowerIconReadyPatch
{
	private static void Postfix(NPower __instance)
	{
		ValencinaPowerIconRefresh.Apply(__instance);
	}
}
