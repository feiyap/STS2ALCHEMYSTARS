using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NHealthBar), "RefreshBlockUi")]
internal static class ValencinaDodgeHealthBarSuppressVanillaBlockBreakPatch
{
	private static bool Prefix(NHealthBar __instance)
	{
		Creature creatureForPatch = DodgeHealthBarOverlay.GetCreatureForPatch(__instance);
		if (creatureForPatch == null || !DodgeHealthBarOverlay.IsSupportedCreature(creatureForPatch))
		{
			return true;
		}
		int num = Math.Max(0, creatureForPatch.Block);
		int num2 = creatureForPatch.GetPower<InstantForesightPower>()?.DodgeValue ?? 0;
		if (num <= 0)
		{
			return num2 <= 0;
		}
		return true;
	}
}
