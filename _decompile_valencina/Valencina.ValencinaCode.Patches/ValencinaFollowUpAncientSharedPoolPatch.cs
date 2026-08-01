using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal static class ValencinaFollowUpAncientSharedPoolPatch
{
	private static void Postfix(ref IEnumerable<AncientEventModel> __result)
	{
		__result = ValencinaSpecialAncientPoolGuard.FilterSharedAncientPool(__result);
	}
}
