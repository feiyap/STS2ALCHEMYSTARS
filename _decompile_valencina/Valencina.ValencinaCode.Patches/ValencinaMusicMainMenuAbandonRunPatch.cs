using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NMainMenu), "AbandonRun")]
internal static class ValencinaMusicMainMenuAbandonRunPatch
{
	private static void Prefix()
	{
		ValencinaRunTeardownGuard.BeforeRunTeardown("NMainMenu.AbandonRun");
	}
}
