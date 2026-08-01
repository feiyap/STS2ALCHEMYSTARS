using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal static class UngezieferKaiserBossNodePathPatch
{
	private static void Postfix(EncounterModel __instance, ref string __result)
	{
		if (UngezieferKaiserEncounterReplacementPatch.ShouldUseKaiserAssets(__instance))
		{
			__result = "res://Valencina/images/ui/run_history/ungeziefer_kaiser_encounter";
		}
	}
}
