using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(RunManager), "AbandonInternal")]
internal static class ValencinaRunManagerAbandonInternalPatch
{
	private static void Prefix()
	{
		ValencinaRunTeardownGuard.BeforeRunTeardown("RunManager.AbandonInternal");
	}
}
