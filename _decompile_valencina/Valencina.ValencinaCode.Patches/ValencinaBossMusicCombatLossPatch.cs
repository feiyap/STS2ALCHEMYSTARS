using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using Valencina.ValencinaCode.Audio;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(CombatManager), "LoseCombat")]
internal static class ValencinaBossMusicCombatLossPatch
{
	private static void Prefix()
	{
		ValencinaRunTeardownGuard.BeforeCombatLoss();
	}

	private static void Postfix()
	{
		ValencinaMusicManager.StopBossMusicAfterCombat();
	}
}
