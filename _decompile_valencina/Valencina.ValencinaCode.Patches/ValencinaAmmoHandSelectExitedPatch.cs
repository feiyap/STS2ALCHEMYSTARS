using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Valencina.ValencinaCode.UI;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NCombatUi), "OnHandSelectModeExited")]
public static class ValencinaAmmoHandSelectExitedPatch
{
	public static void Postfix()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		NCombatRoom instance = NCombatRoom.Instance;
		if (instance != null && (int)instance.Mode <= 0)
		{
			if (!ValencinaAmmoCombatStartPatch.WasInitializedAsActiveCombat(NCombatRoom.Instance))
			{
				ValencinaAmmoCombatStartPatch.TryStartInitialization(NCombatRoom.Instance, "hand-select-exited-lazy");
			}
			else
			{
				AmmoUiSync.RefreshAll(showFallbackLabel: false);
			}
		}
	}
}
