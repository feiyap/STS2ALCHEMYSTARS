using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
public static class AmmoSpendPreviewReloadPatch
{
	private static MethodBase? TargetMethod()
	{
		return AmmoSpendPreviewPatchTargets.NCardMethod("Reload");
	}

	private static bool Prepare()
	{
		return TargetMethod() != null;
	}

	private static void Postfix(NCard __instance)
	{
		try
		{
			AmmoSpendPreviewPatch.Refresh(__instance);
		}
		catch (Exception ex)
		{
			MainFile.Logger.Info("[AmmoSpendPreviewReloadPatch] failed to update card ammo preview: " + ex.Message, 1);
		}
	}
}
