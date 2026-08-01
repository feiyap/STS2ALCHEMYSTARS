using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NCombatUi), "Enable")]
internal static class UngezieferKaiserPhaseChoiceCombatUiEnablePatch
{
	private static bool Prefix()
	{
		return !UngezieferKaiserPhaseChoiceCombatResumePatch.IsRestoringPhaseCombatRoom;
	}
}
