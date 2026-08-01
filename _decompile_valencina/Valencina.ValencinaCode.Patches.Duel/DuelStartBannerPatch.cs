using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.addons.mega_text;
using Valencina.ValencinaCode.Encounters;

namespace Valencina.ValencinaCode.Patches.Duel;

[HarmonyPatch(typeof(NCombatStartBanner), "_Ready")]
internal static class DuelStartBannerPatch
{
	private static void Postfix(NCombatStartBanner __instance)
	{
		RunState obj = RunManager.Instance.DebugOnlyGetState();
		AbstractRoom obj2 = ((obj != null) ? ((IRunState)obj).CurrentRoom : null);
		CombatRoom val = (CombatRoom)(object)((obj2 is CombatRoom) ? obj2 : null);
		if (val != null && val.Encounter is DuelEncounter)
		{
			((Node)__instance).GetNode<MegaLabel>(NodePath.op_Implicit("Label")).SetTextAutoSize(((EncounterModel)ModelDb.Encounter<DuelEncounter>()).Title.GetFormattedText());
		}
	}
}
