using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Events;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(AncientEventModel), "get_RunHistoryIconOutlinePath")]
internal static class ValencinaRienRunHistoryIconOutlineSafePatch
{
	private static bool Prefix(AncientEventModel __instance, ref string? __result)
	{
		string text = ((__instance is ThumbAdvisor) ? "res://Valencina/images/ui/run_history/thumb_advisor_outline.png" : ((__instance is LimbusCompanyHeadquarters) ? "res://Valencina/images/ui/run_history/limbus_company_headquarters_outline.png" : ((__instance is Rien) ? "res://Valencina/images/ui/run_history/rien_outline.png" : ((!(__instance is Stars)) ? null : "res://Valencina/images/events/stars_background.webp"))));
		string text2 = text;
		if (text2 == null)
		{
			return true;
		}
		__result = text2;
		return false;
	}
}
