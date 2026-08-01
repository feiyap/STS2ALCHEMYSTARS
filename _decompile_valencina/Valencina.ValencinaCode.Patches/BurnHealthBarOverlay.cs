using System;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Patches;

internal static class BurnHealthBarOverlay
{
	private const string BurnForegroundName = "ValencinaBurnForeground";

	private const float MinSize = 12f;

	private static readonly Color BurnColor = new Color("FFD43A");

	private static readonly FieldInfo? CreatureField = AccessTools.Field(typeof(NHealthBar), "_creature");

	private static readonly FieldInfo? HpForegroundField = AccessTools.Field(typeof(NHealthBar), "_hpForeground");

	private static readonly FieldInfo? HpForegroundContainerField = AccessTools.Field(typeof(NHealthBar), "_hpForegroundContainer");

	private static readonly FieldInfo? ExpectedMaxFgWidthField = AccessTools.Field(typeof(NHealthBar), "_expectedMaxFgWidth");

	public static void Ensure(NHealthBar healthBar)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (GetBurnForeground(healthBar) != null)
		{
			return;
		}
		NinePatchRect nodeOrNull = ((Node)healthBar).GetNodeOrNull<NinePatchRect>(NodePath.op_Implicit("%PoisonForeground"));
		if (nodeOrNull == null)
		{
			return;
		}
		Node parent = ((Node)nodeOrNull).GetParent();
		if (parent == null)
		{
			return;
		}
		Node obj = ((Node)nodeOrNull).Duplicate(15);
		NinePatchRect val = (NinePatchRect)(object)((obj is NinePatchRect) ? obj : null);
		if (val != null)
		{
			((Node)val).Name = StringName.op_Implicit("ValencinaBurnForeground");
			((Node)val).UniqueNameInOwner = false;
			((CanvasItem)val).Visible = false;
			((CanvasItem)val).SelfModulate = BurnColor;
			((CanvasItem)val).Modulate = Colors.White;
			((Control)val).MouseFilter = (MouseFilterEnum)2;
			parent.AddChild((Node)(object)val, false, (InternalMode)0);
			Control hpForeground = GetHpForeground(healthBar);
			if (hpForeground != null)
			{
				parent.MoveChild((Node)(object)val, ((Node)hpForeground).GetIndex(false));
			}
		}
	}

	public static void Refresh(NHealthBar healthBar)
	{
		Ensure(healthBar);
		NinePatchRect burnForeground = GetBurnForeground(healthBar);
		if (burnForeground == null)
		{
			return;
		}
		Creature creature = GetCreature(healthBar);
		if (creature != null)
		{
			Control hpForeground = GetHpForeground(healthBar);
			if (hpForeground != null)
			{
				BurnPower power = creature.GetPower<BurnPower>();
				int num = (int)(((decimal?)((power != null) ? new int?(((PowerModel)power).Amount) : ((int?)null))) ?? 0m);
				if (creature.CurrentHp <= 0 || creature.MaxHp <= 0 || num <= 0)
				{
					((CanvasItem)burnForeground).Visible = false;
					((CanvasItem)hpForeground).Visible = true;
					return;
				}
				float maxFgWidth = GetMaxFgWidth(healthBar);
				if (maxFgWidth <= 0f)
				{
					((CanvasItem)burnForeground).Visible = false;
					((CanvasItem)hpForeground).Visible = true;
					return;
				}
				float offsetRight = GetFgWidth(creature, creature.CurrentHp, maxFgWidth) - maxFgWidth;
				int amount = Math.Max(0, creature.CurrentHp - num);
				float fgWidth = GetFgWidth(creature, amount, maxFgWidth);
				float num2 = fgWidth - maxFgWidth;
				((CanvasItem)burnForeground).Visible = true;
				((Control)burnForeground).OffsetRight = offsetRight;
				((CanvasItem)hpForeground).Visible = true;
				if (num >= creature.CurrentHp)
				{
					((Control)burnForeground).OffsetLeft = 0f;
					((CanvasItem)hpForeground).Visible = false;
					return;
				}
				int patchMarginLeft = burnForeground.PatchMarginLeft;
				((Control)burnForeground).OffsetLeft = Math.Max(0f, fgWidth - (float)patchMarginLeft);
				if (((CanvasItem)hpForeground).Visible && num2 < hpForeground.OffsetRight)
				{
					hpForeground.OffsetRight = num2;
				}
				return;
			}
		}
		((CanvasItem)burnForeground).Visible = false;
	}

	private static NinePatchRect? GetBurnForeground(NHealthBar healthBar)
	{
		return (NinePatchRect?)(((object)((Node)healthBar).GetNodeOrNull<NinePatchRect>(NodePath.op_Implicit("%ValencinaBurnForeground"))) ?? ((object)/*isinst with value type is only supported in some contexts*/));
	}

	private static Creature? GetCreature(NHealthBar healthBar)
	{
		object? obj = CreatureField?.GetValue(healthBar);
		return (Creature?)((obj is Creature) ? obj : null);
	}

	private static Control? GetHpForeground(NHealthBar healthBar)
	{
		object? obj = HpForegroundField?.GetValue(healthBar);
		return (Control?)((obj is Control) ? obj : null);
	}

	private static float GetMaxFgWidth(NHealthBar healthBar)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (ExpectedMaxFgWidthField?.GetValue(healthBar) is float num && num > 0f)
		{
			return num;
		}
		object? obj = HpForegroundContainerField?.GetValue(healthBar);
		Control val = (Control)((obj is Control) ? obj : null);
		if (val != null)
		{
			return val.Size.X;
		}
		return 0f;
	}

	private static float GetFgWidth(Creature creature, int amount, float maxFgWidth)
	{
		return Math.Max((float)amount / (float)creature.MaxHp * maxFgWidth, (creature.CurrentHp > 0) ? 12f : 0f);
	}
}
