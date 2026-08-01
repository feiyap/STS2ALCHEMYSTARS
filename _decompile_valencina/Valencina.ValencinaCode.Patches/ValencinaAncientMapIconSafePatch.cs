using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Events;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(AncientEventModel), "get_MapIcon")]
internal static class ValencinaAncientMapIconSafePatch
{
	private static bool Prefix(AncientEventModel __instance, ref Texture2D __result)
	{
		if (!(__instance is Stars))
		{
			return true;
		}
		__result = (Texture2D)(object)PreloadManager.Cache.GetCompressedTexture2D("res://Valencina/images/events/stars_background.webp");
		return false;
	}
}
