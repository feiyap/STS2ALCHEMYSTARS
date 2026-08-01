using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;
using Valencina.ValencinaCode.Audio;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NGame), "Quit")]
internal static class ValencinaMusicQuitPatch
{
	private static void Prefix()
	{
		ValencinaMusicManager.StopAllModMusicImmediatelyForShutdown();
	}
}
