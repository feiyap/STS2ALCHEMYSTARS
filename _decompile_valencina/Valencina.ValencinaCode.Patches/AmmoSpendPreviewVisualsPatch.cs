using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
public static class AmmoSpendPreviewVisualsPatch
{
	private static MethodBase? TargetMethod()
	{
		return AmmoSpendPreviewPatchTargets.NCardMethod("UpdateVisuals", typeof(PileType), typeof(CardPreviewMode));
	}

	private static bool Prepare()
	{
		return TargetMethod() != null;
	}

	private static void Postfix(NCard __instance, PileType pileType, CardPreviewMode previewMode)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			AmmoSpendPreviewPatch.Refresh(__instance, pileType);
		}
		catch (Exception ex)
		{
			MainFile.Logger.Info("[AmmoSpendPreviewVisualsPatch] failed to update card ammo preview: " + ex.Message, 1);
		}
	}
}
