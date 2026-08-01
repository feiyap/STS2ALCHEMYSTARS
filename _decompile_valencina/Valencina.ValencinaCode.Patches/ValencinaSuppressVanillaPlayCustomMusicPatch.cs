using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Audio;
using Valencina.ValencinaCode.Audio;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NRunMusicController), "PlayCustomMusic")]
internal static class ValencinaSuppressVanillaPlayCustomMusicPatch
{
	private static bool Prefix()
	{
		return !ValencinaMusicManager.IsOverrideActive;
	}
}
