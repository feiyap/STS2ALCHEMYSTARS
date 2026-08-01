using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NGame), "StartNewSingleplayerRun")]
internal static class ValencinaWarDifficultySingleplayerStartPatch
{
	private static void Prefix(ref int ascensionLevel)
	{
		if (ValencinaWarDifficulty.ConsumePendingWarChoice())
		{
			ascensionLevel = 11;
		}
	}
}
