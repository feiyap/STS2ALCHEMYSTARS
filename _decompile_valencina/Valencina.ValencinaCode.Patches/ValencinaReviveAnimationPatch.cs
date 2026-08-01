using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Valencina.ValencinaCode.Character;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NCreature), "StartReviveAnim")]
internal static class ValencinaReviveAnimationPatch
{
	private static void Postfix(NCreature __instance)
	{
		Player player = __instance.Entity.Player;
		if (((player != null) ? player.Character : null) is Valencina.ValencinaCode.Character.Valencina && !ValencinaAnimation.PlayOn(__instance, "revive", allowOverride: true))
		{
			ValencinaAnimation.ResetIfAlive(__instance, forceIdle: true);
		}
	}
}
