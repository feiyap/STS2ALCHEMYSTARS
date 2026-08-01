using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.addons.mega_text;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NPower), "RefreshAmount")]
internal static class BreathingMethodPowerAmountLabelPatch
{
	private const string IntensityLabelName = "ValencinaBreathingMethodIntensityAmount";

	private const string ChargesLabelName = "ValencinaBreathingMethodChargesAmount";

	private const float InwardOffset = 3f;

	private static readonly Color IntensityColor = new Color(1f, 0.82f, 0.28f, 1f);

	private static readonly Color IntensityOutlineColor = new Color(0.16f, 0.09f, 0.02f, 1f);

	private static readonly Color ChargesColor = new Color(0.72f, 0.96f, 1f, 1f);

	private static readonly Color ChargesOutlineColor = new Color(0.02f, 0.07f, 0.11f, 1f);

	[HarmonyPostfix]
	[HarmonyPriority(0)]
	private static void Postfix(NPower __instance)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		BreathingMethodPower breathingMethodPower;
		try
		{
			breathingMethodPower = __instance.Model as BreathingMethodPower;
		}
		catch
		{
			return;
		}
		MegaLabel nodeOrNull = ((Node)__instance).GetNodeOrNull<MegaLabel>(NodePath.op_Implicit("%AmountLabel"));
		if (breathingMethodPower == null)
		{
			SetBreathingLabelsVisible(nodeOrNull, visible: false);
		}
		else if (nodeOrNull != null)
		{
			MegaLabel orCreateSiblingAmountLabel = GetOrCreateSiblingAmountLabel(nodeOrNull, "ValencinaBreathingMethodIntensityAmount");
			MegaLabel orCreateSiblingAmountLabel2 = GetOrCreateSiblingAmountLabel(nodeOrNull, "ValencinaBreathingMethodChargesAmount");
			if (orCreateSiblingAmountLabel != null && orCreateSiblingAmountLabel2 != null)
			{
				nodeOrNull.SetTextAutoSize(string.Empty);
				Vector2 position = ((Control)nodeOrNull).Position;
				float num = ((Control)__instance).Size.X * 0.5f;
				float num2 = ((Control)nodeOrNull).Position.X + ((Control)nodeOrNull).Size.X;
				float num3 = 2f * (num - num2);
				((Control)orCreateSiblingAmountLabel).Position = position + new Vector2(num3 + 3f, 0f);
				((Control)orCreateSiblingAmountLabel2).Position = position + new Vector2(-3f, 0f);
				((Control)orCreateSiblingAmountLabel).Size = ((Control)nodeOrNull).Size;
				((Control)orCreateSiblingAmountLabel2).Size = ((Control)nodeOrNull).Size;
				((Control)orCreateSiblingAmountLabel).Scale = ((Control)nodeOrNull).Scale;
				((Control)orCreateSiblingAmountLabel2).Scale = ((Control)nodeOrNull).Scale;
				((Control)orCreateSiblingAmountLabel).PivotOffset = ((Control)nodeOrNull).PivotOffset;
				((Control)orCreateSiblingAmountLabel2).PivotOffset = ((Control)nodeOrNull).PivotOffset;
				((CanvasItem)orCreateSiblingAmountLabel).ZIndex = ((CanvasItem)nodeOrNull).ZIndex;
				((CanvasItem)orCreateSiblingAmountLabel2).ZIndex = ((CanvasItem)nodeOrNull).ZIndex;
				ApplyBreathingNumberColors(orCreateSiblingAmountLabel, orCreateSiblingAmountLabel2);
				((CanvasItem)orCreateSiblingAmountLabel).Visible = breathingMethodPower.Charges > 0;
				((CanvasItem)orCreateSiblingAmountLabel2).Visible = breathingMethodPower.Charges > 0;
				orCreateSiblingAmountLabel.SetTextAutoSize((breathingMethodPower.Charges > 0) ? breathingMethodPower.Intensity.ToString() : string.Empty);
				orCreateSiblingAmountLabel2.SetTextAutoSize((breathingMethodPower.Charges > 0) ? breathingMethodPower.Charges.ToString() : string.Empty);
			}
		}
	}

	private static MegaLabel? GetOrCreateSiblingAmountLabel(MegaLabel template, string name)
	{
		Node parent = ((Node)template).GetParent();
		if (parent == null)
		{
			return null;
		}
		MegaLabel nodeOrNull = parent.GetNodeOrNull<MegaLabel>(NodePath.op_Implicit(name));
		if (nodeOrNull != null)
		{
			return nodeOrNull;
		}
		Node obj = ((Node)template).Duplicate(15);
		MegaLabel val = (MegaLabel)(object)((obj is MegaLabel) ? obj : null);
		if (val == null)
		{
			return null;
		}
		((Node)val).Name = StringName.op_Implicit(name);
		((Node)val).UniqueNameInOwner = false;
		((Control)val).MouseFilter = (MouseFilterEnum)2;
		parent.AddChild((Node)(object)val, false, (InternalMode)0);
		return val;
	}

	private static void ApplyBreathingNumberColors(MegaLabel intensityLabel, MegaLabel chargesLabel)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		((Control)intensityLabel).AddThemeColorOverride(StringName.op_Implicit("font_color"), IntensityColor);
		((Control)intensityLabel).AddThemeColorOverride(StringName.op_Implicit("font_outline_color"), IntensityOutlineColor);
		((Control)chargesLabel).AddThemeColorOverride(StringName.op_Implicit("font_color"), ChargesColor);
		((Control)chargesLabel).AddThemeColorOverride(StringName.op_Implicit("font_outline_color"), ChargesOutlineColor);
	}

	private static void SetBreathingLabelsVisible(MegaLabel? amountLabel, bool visible)
	{
		Node val = ((amountLabel != null) ? ((Node)amountLabel).GetParent() : null);
		if (val == null)
		{
			return;
		}
		MegaLabel nodeOrNull = val.GetNodeOrNull<MegaLabel>(NodePath.op_Implicit("ValencinaBreathingMethodIntensityAmount"));
		if (nodeOrNull != null)
		{
			((CanvasItem)nodeOrNull).Visible = visible;
			if (!visible)
			{
				nodeOrNull.SetTextAutoSize(string.Empty);
			}
		}
		MegaLabel nodeOrNull2 = val.GetNodeOrNull<MegaLabel>(NodePath.op_Implicit("ValencinaBreathingMethodChargesAmount"));
		if (nodeOrNull2 != null)
		{
			((CanvasItem)nodeOrNull2).Visible = visible;
			if (!visible)
			{
				nodeOrNull2.SetTextAutoSize(string.Empty);
			}
		}
	}
}
