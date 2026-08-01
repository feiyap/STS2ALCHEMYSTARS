using Valencina.ValencinaCode.Audio;
using Valencina.ValencinaCode.UI;

namespace Valencina.ValencinaCode.Patches;

internal static class ValencinaRunTeardownGuard
{
	internal static void BeforeCombatEnds(string reason)
	{
		ValencinaAnimation.QuiesceForCombatEndAfterGrace(reason);
		AmmoUiSync.DestroyCombatUi();
	}

	internal static void BeforeCombatLoss()
	{
		AmmoUiSync.DestroyCombatUi();
	}

	internal static void BeforeRunTeardown(string reason)
	{
		ValencinaAnimation.QuiesceForRunTeardown(reason);
		AmmoUiSync.DestroyCombatUi();
		ValencinaMusicManager.StopAllModMusicForMainMenu();
	}
}
