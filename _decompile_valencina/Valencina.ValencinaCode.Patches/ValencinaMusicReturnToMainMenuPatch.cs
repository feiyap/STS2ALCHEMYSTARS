using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NGame), "ReturnToMainMenu")]
internal static class ValencinaMusicReturnToMainMenuPatch
{
	private static void Prefix()
	{
		ValencinaRunTeardownGuard.BeforeRunTeardown("NGame.ReturnToMainMenu");
	}
}
