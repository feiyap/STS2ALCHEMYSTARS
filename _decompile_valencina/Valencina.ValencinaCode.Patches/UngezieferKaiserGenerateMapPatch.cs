using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(RunManager), "GenerateMap")]
internal static class UngezieferKaiserGenerateMapPatch
{
	private static void Prefix(RunManager __instance)
	{
		if (UngezieferKaiserFinalBossController.TryGetRunState(__instance, out IRunState runState))
		{
			UngezieferKaiserFinalBossController.EnsureAct4Slot(runState, log: false);
			UngezieferKaiserFinalBossController.RepairAccidentalKaiserBossSelections(runState, log: false);
		}
	}
}
