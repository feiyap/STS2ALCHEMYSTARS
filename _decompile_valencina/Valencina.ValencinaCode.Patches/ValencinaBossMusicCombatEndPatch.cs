using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using Valencina.ValencinaCode.Audio;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(CombatManager), "EndCombatInternal")]
internal static class ValencinaBossMusicCombatEndPatch
{
	private static void Prefix()
	{
		ValencinaRunTeardownGuard.BeforeCombatEnds("CombatManager.EndCombatInternal");
		ValencinaMusicManager.StopBossMusicAfterCombat(stopTransientAudioImmediately: false);
	}
}
