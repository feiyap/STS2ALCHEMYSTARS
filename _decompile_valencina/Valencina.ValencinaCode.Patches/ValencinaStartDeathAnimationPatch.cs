using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Valencina.ValencinaCode.Character;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NCreature), "StartDeathAnim")]
internal static class ValencinaStartDeathAnimationPatch
{
	private static void Prefix(NCreature __instance)
	{
		Player player = __instance.Entity.Player;
		if (((player != null) ? player.Character : null) is Valencina.ValencinaCode.Character.Valencina)
		{
			ValencinaAnimation.PrepareForDeath(__instance);
		}
	}

	private static void Postfix(NCreature __instance, ref float __result)
	{
		Player player = __instance.Entity.Player;
		if (((player != null) ? player.Character : null) is Valencina.ValencinaCode.Character.Valencina && ValencinaAnimation.PlayDeathFromVanillaDeathFlow(__instance))
		{
			__result = Math.Max(__result, 1.05f);
		}
	}
}
