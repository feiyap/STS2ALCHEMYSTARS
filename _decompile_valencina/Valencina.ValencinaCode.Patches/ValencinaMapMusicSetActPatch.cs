using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Audio;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(RunManager), "SetActInternal")]
internal static class ValencinaMapMusicSetActPatch
{
	private static void Postfix()
	{
		ValencinaMusicManager.OnActChanged();
	}
}
