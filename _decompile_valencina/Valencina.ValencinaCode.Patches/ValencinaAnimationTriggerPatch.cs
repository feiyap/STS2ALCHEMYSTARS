using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Valencina.ValencinaCode.Character;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NCreature), "SetAnimationTrigger")]
internal static class ValencinaAnimationTriggerPatch
{
	private static void Postfix(NCreature __instance, string trigger)
	{
		Player player = __instance.Entity.Player;
		if (((player != null) ? player.Character : null) is Valencina.ValencinaCode.Character.Valencina && !ValencinaAnimation.AreCustomVisualsSuppressedForTeardown() && string.Equals(trigger, "Attack", StringComparison.Ordinal))
		{
			ValencinaAnimation.PlayAttackFromNode(__instance);
		}
	}
}
