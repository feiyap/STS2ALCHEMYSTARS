using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(ActModel), "ValidateRoomsAfterLoad")]
internal static class ValencinaFollowUpAncientValidateRoomsAfterLoadPatch
{
	private static void Postfix(Rng rng)
	{
		if (RunManager.Instance == null || !UngezieferKaiserFinalBossController.TryGetRunState(RunManager.Instance, out IRunState runState))
		{
			return;
		}
		foreach (ActModel act in runState.Acts)
		{
			ValencinaSpecialAncientPoolGuard.RepairGeneratedAncient(runState, act, rng, log: true);
		}
	}
}
