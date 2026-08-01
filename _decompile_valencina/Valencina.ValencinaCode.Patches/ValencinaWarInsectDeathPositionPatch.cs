using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NCreature), "StartDeathAnim")]
internal static class ValencinaWarInsectDeathPositionPatch
{
	private static void Prefix(NCreature __instance)
	{
		ValencinaWarInsectSpawnPatch.CaptureDeathPosition(__instance);
	}
}
