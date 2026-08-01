using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NCard), "_Ready")]
public static class AmmoSpendPreviewReadyPatch
{
	private static void Postfix(NCard __instance)
	{
		try
		{
			AmmoSpendPreviewPatch.Refresh(__instance);
		}
		catch (Exception ex)
		{
			MainFile.Logger.Info("[AmmoSpendPreviewReadyPatch] failed to update card ammo preview: " + ex.Message, 1);
		}
	}
}
