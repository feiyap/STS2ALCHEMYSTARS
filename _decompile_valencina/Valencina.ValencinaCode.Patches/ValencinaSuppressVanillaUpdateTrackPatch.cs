using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Audio;
using Valencina.ValencinaCode.Audio;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
internal static class ValencinaSuppressVanillaUpdateTrackPatch
{
	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(typeof(NRunMusicController), "UpdateTrack", Type.EmptyTypes, (Type[])null);
	}

	private static bool Prefix()
	{
		return !ValencinaMusicManager.IsOverrideActive;
	}
}
