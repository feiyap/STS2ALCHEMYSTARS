using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(RunManager), "EnterAct")]
internal static class UngezieferKaiserEnterActPatch
{
	private static void Prefix(RunManager __instance, int currentActIndex)
	{
		if (UngezieferKaiserFinalBossController.TryGetRunState(__instance, out IRunState runState) && currentActIndex >= 0 && currentActIndex < runState.Acts.Count && UngezieferKaiserFinalBossController.IsValencinaAct4(runState.Acts[currentActIndex]))
		{
			UngezieferKaiserFinalBossController.EnsureAct4Slot(runState, log: true);
		}
	}
}
