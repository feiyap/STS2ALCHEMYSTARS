using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NGame), "ReturnToMainMenuAfterRun")]
internal static class ValencinaMusicReturnToMainMenuAfterRunPatch
{
	private static void Prefix()
	{
		ValencinaRunTeardownGuard.BeforeRunTeardown("NGame.ReturnToMainMenuAfterRun");
	}
}
