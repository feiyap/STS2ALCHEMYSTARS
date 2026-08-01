using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(ActModel), "ValidateRoomsAfterLoad")]
internal static class UngezieferKaiserValidateRoomsAfterLoadPatch
{
	private static void Postfix()
	{
		if (RunManager.Instance != null && UngezieferKaiserFinalBossController.TryGetRunState(RunManager.Instance, out IRunState runState))
		{
			UngezieferKaiserFinalBossController.RepairAccidentalKaiserBossSelections(runState, log: true);
			UngezieferKaiserFinalBossController.EnsureAct4Slot(runState, log: true);
		}
	}
}
