using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NAscensionPanel), "SetMaxAscension")]
internal static class ValencinaWarDifficultyMaxPatch
{
	private static void Prefix(NAscensionPanel __instance, ref int maxAscension)
	{
		if (FindCharacterSelectScreen((Node)(object)__instance) != null && ValencinaWarDifficulty.ShouldExpose(maxAscension))
		{
			maxAscension = 11;
		}
	}

	private static NCharacterSelectScreen? FindCharacterSelectScreen(Node node)
	{
		for (Node val = node; val != null; val = val.GetParent())
		{
			NCharacterSelectScreen val2 = (NCharacterSelectScreen)(object)((val is NCharacterSelectScreen) ? val : null);
			if (val2 != null)
			{
				return val2;
			}
		}
		return null;
	}
}
