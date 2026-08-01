using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(ActModel), "GenerateRooms")]
internal static class ValencinaFollowUpAncientGenerateRoomsPatch
{
	private static void Postfix(ActModel __instance, Rng rng)
	{
		if (RunManager.Instance != null && UngezieferKaiserFinalBossController.TryGetRunState(RunManager.Instance, out IRunState runState))
		{
			ValencinaSpecialAncientPoolGuard.RepairGeneratedAncient(runState, __instance, rng, log: false);
		}
	}
}
