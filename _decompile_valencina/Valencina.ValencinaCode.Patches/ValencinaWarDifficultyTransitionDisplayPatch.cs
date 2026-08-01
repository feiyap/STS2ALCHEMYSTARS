using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NAscensionPanel), "SetAscensionLevel")]
internal static class ValencinaWarDifficultyTransitionDisplayPatch
{
	private static void Prefix(ref int ascension)
	{
		if (ascension == 10 && ValencinaWarDifficulty.HasPendingWarChoice)
		{
			ascension = 11;
		}
	}
}
