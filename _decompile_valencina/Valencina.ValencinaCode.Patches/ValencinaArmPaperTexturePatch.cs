using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal static class ValencinaArmPaperTexturePatch
{
	public static bool Prefix(CharacterModel __instance, ref Texture2D __result)
	{
		return ValencinaMultiplayerHandTexture.TryResolve(__instance, "res://Valencina/images/ui/hands/multiplayer_hand_valencina_paper.png", ref __result);
	}
}
