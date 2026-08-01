using System;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using Valencina.ValencinaCode.Cards;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
public static class AmmoSpendPreviewEnchantmentPatch
{
	private static readonly FieldInfo? DefaultEnchantmentPositionField = AccessTools.Field(typeof(NCard), "_defaultEnchantmentPosition");

	private static MethodBase? TargetMethod()
	{
		return AmmoSpendPreviewPatchTargets.NCardMethod("UpdateEnchantmentVisuals");
	}

	private static bool Prepare()
	{
		return TargetMethod() != null;
	}

	private static void Postfix(NCard __instance)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (__instance.Model is ValencinaCard valencinaCard && !((CardModel)valencinaCard).HasStarCostX && valencinaCard.ShowAmmoSpendPreview)
			{
				Control nodeOrNull = ((Node)__instance).GetNodeOrNull<Control>(NodePath.op_Implicit("%Enchantment"));
				if (nodeOrNull != null && ((CanvasItem)nodeOrNull).Visible && !(DefaultEnchantmentPositionField == null) && DefaultEnchantmentPositionField.GetValue(__instance) is Vector2 position)
				{
					nodeOrNull.Position = position;
				}
			}
		}
		catch (Exception ex)
		{
			MainFile.Logger.Info("[AmmoSpendPreviewEnchantmentPatch] failed to update enchantment position: " + ex.Message, 1);
		}
	}
}
