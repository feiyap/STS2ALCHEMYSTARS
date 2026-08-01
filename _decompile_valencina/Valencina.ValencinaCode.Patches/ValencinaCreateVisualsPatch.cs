using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Valencina.ValencinaCode.Character;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(CharacterModel), "CreateVisuals")]
internal static class ValencinaCreateVisualsPatch
{
	private static bool Prefix(CharacterModel __instance, ref NCreatureVisuals __result)
	{
		if (!(__instance is Valencina.ValencinaCode.Character.Valencina))
		{
			return true;
		}
		PackedScene val = ValencinaScenePaths.LoadScene(MainFile.CharacterVisualSceneCandidates);
		if (val == null)
		{
			return true;
		}
		__result = val.Instantiate<NCreatureVisuals>((GenEditState)0);
		return false;
	}
}
