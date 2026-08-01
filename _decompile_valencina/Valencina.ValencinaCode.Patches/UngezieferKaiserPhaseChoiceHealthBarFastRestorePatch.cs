using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NCreatureStateDisplay), "AnimateIn")]
internal static class UngezieferKaiserPhaseChoiceHealthBarFastRestorePatch
{
	private static bool Prefix(NCreatureStateDisplay __instance, HealthBarAnimMode mode)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!UngezieferKaiserPhaseChoiceCombatResumePatch.ShouldFastRestoreVisuals)
		{
			return true;
		}
		((CanvasItem)__instance).Visible = true;
		((CanvasItem)__instance).Modulate = Colors.White;
		return false;
	}
}
