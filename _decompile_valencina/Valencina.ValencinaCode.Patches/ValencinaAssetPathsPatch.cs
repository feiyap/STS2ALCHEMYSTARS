using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Character;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal static class ValencinaAssetPathsPatch
{
	private static void Postfix(CharacterModel __instance, ref IEnumerable<string> __result)
	{
		if (__instance is Valencina.ValencinaCode.Character.Valencina)
		{
			__result = __result.Append(MainFile.CharacterVisualScene).Append(MainFile.RestSiteScene).Append(MainFile.AmmoUiScene)
				.Concat(new string[3] { "res://Valencina/images/charui/character_icon_valencina.png", "res://Valencina/scenes/ui/character_icons/valencina_icon.tscn", "res://Valencina/images/charui/map_marker_valencina.png" });
		}
	}
}
