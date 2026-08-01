using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Character;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal static class ValencinaAssetPathsCharacterSelectPatch
{
	private static void Postfix(CharacterModel __instance, ref IEnumerable<string> __result)
	{
		if (__instance is Valencina.ValencinaCode.Character.Valencina)
		{
			__result = __result.Append(MainFile.CharacterSelectBgScene).Concat(new string[5] { "res://Valencina/images/charui/portrait_valencina.png", "res://Valencina/scenes/ui/character_icons/valencina_icon.tscn", "res://Valencina/images/charui/char_select_valencina.png", "res://Valencina/images/charui/char_select_valencina_locked.png", "res://Valencina/images/charui/character_icon_valencina.png" });
		}
	}
}
