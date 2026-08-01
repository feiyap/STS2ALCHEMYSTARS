using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Cards;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(Hook), "BeforeCombatStart")]
internal static class ValencinaCardTrackerClearBeforeCombatPatch
{
	private static void Prefix(IRunState runState, CombatState? combatState)
	{
		ValencinaCard.ClearCombatTurnTrackers();
	}
}
