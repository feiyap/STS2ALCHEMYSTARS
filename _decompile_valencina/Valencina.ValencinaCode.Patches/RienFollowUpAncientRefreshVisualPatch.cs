using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Valencina.ValencinaCode.Settings;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NEventRoom), "RefreshEventState")]
internal static class RienFollowUpAncientRefreshVisualPatch
{
	private static void Postfix(EventModel eventModel)
	{
		string eventEntry;
		if (!ValencinaModConfig.EnableRienFollowUpAncient)
		{
			RienFollowUpAncientDialogueIconState.ClearCurrentIcon();
		}
		else if (RienFollowUpAncientVisualState.TryGet(eventModel, out eventEntry))
		{
			RienFollowUpAncientVisuals.Apply(eventEntry);
		}
	}
}
