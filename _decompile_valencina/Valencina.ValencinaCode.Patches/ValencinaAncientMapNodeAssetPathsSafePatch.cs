using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Events;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(AncientEventModel), "get_MapNodeAssetPaths")]
internal static class ValencinaAncientMapNodeAssetPathsSafePatch
{
	private static bool Prefix(AncientEventModel __instance, ref IEnumerable<string> __result)
	{
		if (!(__instance is Stars))
		{
			return true;
		}
		__result = new _003C_003Ez__ReadOnlySingleElementList<string>("res://Valencina/images/events/stars_background.webp");
		return false;
	}
}
