using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
internal static class ValencinaWarDifficultyTextPatch
{
	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(typeof(NAscensionPanel), "RefreshAscensionText", (Type[])null, (Type[])null);
	}

	private static void Postfix(NAscensionPanel __instance)
	{
		ValencinaWarDifficulty.RefreshWarText(__instance);
	}
}
