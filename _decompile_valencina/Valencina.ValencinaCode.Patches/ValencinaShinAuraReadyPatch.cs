using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Valencina.ValencinaCode.Vfx;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NCreature), "_Ready")]
internal static class ValencinaShinAuraReadyPatch
{
	private static void Postfix(NCreature __instance)
	{
		ShinAuraController.Refresh(__instance);
	}
}
