using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal static class ValencinaArmScissorsTexturePatch
{
	public static bool Prefix(CharacterModel __instance, ref Texture2D __result)
	{
		return ValencinaMultiplayerHandTexture.TryResolve(__instance, "res://Valencina/images/ui/hands/multiplayer_hand_valencina_scissors.png", ref __result);
	}
}
