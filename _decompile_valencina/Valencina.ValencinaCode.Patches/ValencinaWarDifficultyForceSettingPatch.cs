using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using Valencina.ValencinaCode.Settings;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
internal static class ValencinaWarDifficultyForceSettingPatch
{
	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(typeof(StartRunLobby), "BeginRunForAllPlayers", (Type[])null, (Type[])null);
	}

	private static void Prefix(StartRunLobby __instance)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		ValencinaWarDifficulty.CapturePendingRun(__instance);
		if ((int)__instance.NetService.Type != 3 && ValencinaModConfig.EnableWarDifficulty)
		{
			__instance.SyncAscensionChange(11);
		}
	}
}
