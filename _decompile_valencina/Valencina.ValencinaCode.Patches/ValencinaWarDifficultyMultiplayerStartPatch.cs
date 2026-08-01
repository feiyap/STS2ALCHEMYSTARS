using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NGame), "StartNewMultiplayerRun")]
internal static class ValencinaWarDifficultyMultiplayerStartPatch
{
	private static void Prefix(ref int ascensionLevel)
	{
		if (ValencinaWarDifficulty.ConsumePendingWarChoice())
		{
			ascensionLevel = 11;
		}
	}
}
