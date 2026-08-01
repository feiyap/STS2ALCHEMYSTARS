using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Cards;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(Hook), "AfterCombatEnd")]
internal static class ValencinaCardTrackerClearAfterCombatPatch
{
	private static void Prefix(IRunState runState, CombatState? combatState, CombatRoom room)
	{
		ValencinaCard.ClearCombatTurnTrackers();
	}
}
