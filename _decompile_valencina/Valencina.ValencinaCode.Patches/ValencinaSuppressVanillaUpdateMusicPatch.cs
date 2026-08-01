using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Audio;
using Valencina.ValencinaCode.Audio;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NRunMusicController), "UpdateMusic")]
internal static class ValencinaSuppressVanillaUpdateMusicPatch
{
	private static bool Prefix()
	{
		return !ValencinaMusicManager.IsOverrideActive;
	}
}
