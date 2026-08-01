using HarmonyLib;
using MegaCrit.Sts2.Core.Rooms;
using Valencina.ValencinaCode.Encounters;

namespace Valencina.ValencinaCode.Patches.Duel;

[HarmonyPatch(typeof(CombatRoom), "OnCombatEnded")]
internal static class DuelCombatEndedPatch
{
	private static void Postfix(CombatRoom __instance)
	{
		if (__instance.Encounter is DuelEncounter)
		{
			DuelHpMemory.Restore(__instance);
		}
	}
}
