using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
internal static class ValencinaWarDifficultyBeginCapturePatch
{
	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(typeof(StartRunLobby), "BeginRunLocally", (Type[])null, (Type[])null);
	}

	private static void Prefix(StartRunLobby __instance)
	{
		ValencinaWarDifficulty.CapturePendingRun(__instance);
	}
}
