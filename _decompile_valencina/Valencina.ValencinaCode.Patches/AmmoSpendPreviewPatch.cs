using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.addons.mega_text;
using Valencina.ValencinaCode.Cards;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
public static class AmmoSpendPreviewPatch
{
	private const string AmmoIconPath = "res://Valencina/images/powers/ammo_power.png";

	private static readonly Dictionary<TextureRect, Texture2D?> OriginalStarTextures = new Dictionary<TextureRect, Texture2D>();

	private static readonly FieldInfo? StarIconField = AccessTools.Field(typeof(NCard), "_starIcon");

	private static readonly FieldInfo? StarLabelField = AccessTools.Field(typeof(NCard), "_starLabel");

	private static readonly FieldInfo? UnplayableStarIconField = AccessTools.Field(typeof(NCard), "_unplayableStarIcon");

	private static Texture2D? _ammoIcon;

	private static bool _loggedMissingAmmoIcon;

	private static MethodBase? TargetMethod()
	{
		return AmmoSpendPreviewPatchTargets.NCardMethod("UpdateStarCostVisuals", typeof(PileType));
	}

	private static bool Prepare()
	{
		return TargetMethod() != null;
	}

	private static void Postfix(NCard __instance, PileType pileType)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Refresh(__instance, pileType);
		}
		catch (Exception ex)
		{
			MainFile.Logger.Info("[AmmoSpendPreviewPatch] failed to update card ammo preview: " + ex.Message, 1);
		}
	}

	internal static void Refresh(NCard cardNode)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Refresh(cardNode, cardNode.DisplayingPile);
	}

	internal static void Refresh(NCard cardNode, PileType pileType)
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Invalid comparison between Unknown and I4
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		if (!GodotObject.IsInstanceValid((GodotObject)(object)cardNode))
		{
			return;
		}
		if (!(cardNode.Model is ValencinaCard valencinaCard))
		{
			RestoreDefaultStarIcon(cardNode);
			return;
		}
		if (((CardModel)valencinaCard).HasStarCostX)
		{
			RestoreDefaultStarIcon(cardNode);
			return;
		}
		string ammoSpendPreviewText = valencinaCard.AmmoSpendPreviewText;
		if (!valencinaCard.ShowAmmoSpendPreview)
		{
			RestoreDefaultStarIcon(cardNode);
			return;
		}
		int num = Math.Max(0, valencinaCard.AmmoSpendPreviewAmount);
		MegaLabel starLabel = GetStarLabel(cardNode);
		TextureRect starIcon = GetStarIcon(cardNode);
		TextureRect unplayableStarIcon = GetUnplayableStarIcon(cardNode);
		if (starLabel == null || starIcon == null)
		{
			return;
		}
		StoreDefaultStarIcon(starIcon);
		Texture2D ammoIcon = GetAmmoIcon();
		if (ammoIcon != null)
		{
			starIcon.Texture = ammoIcon;
		}
		starLabel.SetTextAutoSize(ammoSpendPreviewText);
		((CanvasItem)starIcon).Visible = true;
		if (unplayableStarIcon != null)
		{
			((CanvasItem)unplayableStarIcon).Visible = false;
		}
		int num2;
		if ((int)pileType == 2)
		{
			Player owner = ((CardModel)valencinaCard).Owner;
			if (((owner != null) ? owner.Creature : null) != null)
			{
				num2 = ((AmmoSystem.CurrentAmmo(((CardModel)valencinaCard).Owner.Creature) < num) ? 1 : 0);
				goto IL_00d4;
			}
		}
		num2 = 0;
		goto IL_00d4;
		IL_00d4:
		bool flag = (byte)num2 != 0;
		((Control)starLabel).AddThemeColorOverride(StringName.op_Implicit("font_color"), flag ? StsColors.red : StsColors.cream);
		((Control)starLabel).AddThemeColorOverride(StringName.op_Implicit("font_outline_color"), flag ? StsColors.unplayableEnergyCostOutline : StsColors.defaultStarCostOutline);
	}

	private static Texture2D? GetAmmoIcon()
	{
		if (IsValid((GodotObject?)(object)_ammoIcon))
		{
			return _ammoIcon;
		}
		_ammoIcon = null;
		_ammoIcon = ResourceLoader.Load<Texture2D>("res://Valencina/images/powers/ammo_power.png", string.Empty, (CacheMode)1);
		if (_ammoIcon == null && !_loggedMissingAmmoIcon)
		{
			_loggedMissingAmmoIcon = true;
			MainFile.Logger.Info("[AmmoSpendPreviewPatch] ammo icon missing: res://Valencina/images/powers/ammo_power.png", 1);
		}
		return _ammoIcon;
	}

	private static void StoreDefaultStarIcon(TextureRect icon)
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)icon) || OriginalStarTextures.ContainsKey(icon))
		{
			return;
		}
		try
		{
			Texture2D texture = icon.Texture;
			OriginalStarTextures[icon] = (IsValid((GodotObject?)(object)texture) ? texture : null);
		}
		catch (ObjectDisposedException)
		{
			OriginalStarTextures[icon] = null;
		}
	}

	private static void RestoreDefaultStarIcon(NCard cardNode)
	{
		TextureRect starIcon = GetStarIcon(cardNode);
		if (starIcon != null && GodotObject.IsInstanceValid((GodotObject)(object)starIcon) && OriginalStarTextures.TryGetValue(starIcon, out Texture2D value))
		{
			if (IsValid((GodotObject?)(object)value))
			{
				starIcon.Texture = value;
			}
			else
			{
				OriginalStarTextures.Remove(starIcon);
			}
		}
		MegaLabel starLabel = GetStarLabel(cardNode);
		if (starLabel != null)
		{
			((Control)starLabel).RemoveThemeColorOverride(StringName.op_Implicit("font_color"));
			((Control)starLabel).RemoveThemeColorOverride(StringName.op_Implicit("font_outline_color"));
		}
	}

	private static MegaLabel? GetStarLabel(NCard cardNode)
	{
		object? obj = StarLabelField?.GetValue(cardNode);
		return (MegaLabel?)(((obj is MegaLabel) ? obj : null) ?? ((Node)cardNode).GetNodeOrNull<MegaLabel>(NodePath.op_Implicit("%StarLabel")));
	}

	private static TextureRect? GetStarIcon(NCard cardNode)
	{
		object? obj = StarIconField?.GetValue(cardNode);
		return (TextureRect?)(((obj is TextureRect) ? obj : null) ?? ((Node)cardNode).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%StarIcon")));
	}

	private static TextureRect? GetUnplayableStarIcon(NCard cardNode)
	{
		object? obj = UnplayableStarIconField?.GetValue(cardNode);
		return (TextureRect?)(((obj is TextureRect) ? obj : null) ?? ((Node)cardNode).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%UnplayableStarIcon")));
	}

	private static bool IsValid(GodotObject? obj)
	{
		if (obj != null)
		{
			return GodotObject.IsInstanceValid(obj);
		}
		return false;
	}
}
