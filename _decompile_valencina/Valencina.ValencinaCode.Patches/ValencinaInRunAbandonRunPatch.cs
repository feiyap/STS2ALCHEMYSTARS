using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
internal static class ValencinaInRunAbandonRunPatch
{
	private static MethodBase? TargetMethod()
	{
		return AccessTools.Method(typeof(NAbandonRunConfirmPopup), "OnYesButtonPressed", (Type[])null, (Type[])null);
	}

	private static void Prefix()
	{
		ValencinaRunTeardownGuard.BeforeRunTeardown("NAbandonRunConfirmPopup.OnYesButtonPressed");
	}
}
