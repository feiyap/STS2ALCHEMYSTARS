using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
internal static class ValencinaWarDifficultyMultiplayerClampPatch
{
	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(typeof(StartRunLobby), "UpdateMaxMultiplayerAscension", (Type[])null, (Type[])null);
	}

	private static void Prefix(StartRunLobby __instance, out bool __state)
	{
		__state = __instance.Ascension == 11;
	}

	private static void Postfix(StartRunLobby __instance, bool __state)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		if (__state && (int)__instance.NetService.Type == 2 && __instance.Ascension != 11)
		{
			__instance.SyncAscensionChange(11);
		}
	}
}
