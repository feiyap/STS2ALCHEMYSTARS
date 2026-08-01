using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Valencina.ValencinaCode.Monsters;

[ScriptPath("res://ValencinaCode/Monsters/Act4EliteCreatureVisuals.cs")]
public class Act4EliteCreatureVisuals : NCreatureVisuals
{
	public class MethodName : MethodName
	{
		public static readonly StringName _Ready = StringName.op_Implicit("_Ready");

		public static readonly StringName PlayAttackVisual = StringName.op_Implicit("PlayAttackVisual");

		public static readonly StringName OnAttackVisualFinished = StringName.op_Implicit("OnAttackVisualFinished");
	}

	public class PropertyName : PropertyName
	{
	}

	public class SignalName : SignalName
	{
	}

	public override void _Ready()
	{
		((NCreatureVisuals)this)._Ready();
		AnimatedSprite2D nodeOrNull = ((Node)this).GetNodeOrNull<AnimatedSprite2D>(NodePath.op_Implicit("%AttackVisuals"));
		if (nodeOrNull != null)
		{
			((CanvasItem)nodeOrNull).Visible = false;
		}
	}

	public void PlayAttackVisual()
	{
		AnimatedSprite2D nodeOrNull = ((Node)this).GetNodeOrNull<AnimatedSprite2D>(NodePath.op_Implicit("%AttackVisuals"));
		if (nodeOrNull != null)
		{
			AnimatedSprite2D nodeOrNull2 = ((Node)this).GetNodeOrNull<AnimatedSprite2D>(NodePath.op_Implicit("%Visuals"));
			if (nodeOrNull2 != null)
			{
				((CanvasItem)nodeOrNull2).Visible = false;
			}
			nodeOrNull.AnimationFinished -= OnAttackVisualFinished;
			nodeOrNull.AnimationFinished += OnAttackVisualFinished;
			((CanvasItem)nodeOrNull).Visible = true;
			nodeOrNull.Frame = 0;
			nodeOrNull.Play(StringName.op_Implicit("attack"), 1f, false);
		}
	}

	private void OnAttackVisualFinished()
	{
		AnimatedSprite2D nodeOrNull = ((Node)this).GetNodeOrNull<AnimatedSprite2D>(NodePath.op_Implicit("%AttackVisuals"));
		if (nodeOrNull != null)
		{
			nodeOrNull.AnimationFinished -= OnAttackVisualFinished;
			((CanvasItem)nodeOrNull).Visible = false;
		}
		AnimatedSprite2D nodeOrNull2 = ((Node)this).GetNodeOrNull<AnimatedSprite2D>(NodePath.op_Implicit("%Visuals"));
		if (nodeOrNull2 != null)
		{
			((CanvasItem)nodeOrNull2).Visible = true;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		return new List<MethodInfo>(3)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.PlayAttackVisual, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.OnAttackVisualFinished, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if ((ref method) == MethodName._Ready && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			((Node)this)._Ready();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.PlayAttackVisual && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			PlayAttackVisual();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.OnAttackVisualFinished && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			OnAttackVisualFinished();
			ret = default(godot_variant);
			return true;
		}
		return ((NCreatureVisuals)this).InvokeGodotClassMethod(ref method, args, ref ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if ((ref method) == MethodName._Ready)
		{
			return true;
		}
		if ((ref method) == MethodName.PlayAttackVisual)
		{
			return true;
		}
		if ((ref method) == MethodName.OnAttackVisualFinished)
		{
			return true;
		}
		return ((NCreatureVisuals)this).HasGodotClassMethod(ref method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		((NCreatureVisuals)this).SaveGodotObjectData(info);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		((NCreatureVisuals)this).RestoreGodotObjectData(info);
	}
}
