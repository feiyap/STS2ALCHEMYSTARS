using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
internal static class ValencinaMusicLoadMainMenuPatch
{
	private static MethodBase? TargetMethod()
	{
		return AccessTools.Method(typeof(NGame), "LoadMainMenu", new Type[1] { typeof(bool) }, (Type[])null);
	}

	private static void Prefix()
	{
		ValencinaRunTeardownGuard.BeforeRunTeardown("NGame.LoadMainMenu");
	}
}
