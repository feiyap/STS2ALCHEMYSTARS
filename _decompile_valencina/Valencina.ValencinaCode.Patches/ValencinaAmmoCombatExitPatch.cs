using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Valencina.ValencinaCode.Systems;
using Valencina.ValencinaCode.UI;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NCombatRoom), "_ExitTree")]
public static class ValencinaAmmoCombatExitPatch
{
	public static void Postfix(NCombatRoom __instance)
	{
		ValencinaAnimation.ClearCombatRoomState(__instance);
		if (!ValencinaAmmoCombatStartPatch.RemoveInitializedRoom(__instance))
		{
			ValencinaProbeLog.Info("ammo-room-exit-skip-uninitialized", "Skipped ammo cleanup for combat room not initialized by ammo system. " + ValencinaAmmoCombatStartPatch.DescribeRoom(__instance), 20);
			return;
		}
		ValencinaProbeLog.Info("ammo-room-exit-active", "Ammo cleanup accepted for initialized active combat room. " + ValencinaAmmoCombatStartPatch.DescribeRoom(__instance), 20);
		AmmoUiSync.DestroyCombatUi();
		AmmoSystem.LeaveCombat();
	}
}
