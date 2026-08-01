using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.RestSite;

namespace Valencina.ValencinaCode.Character;

[ScriptPath("res://ValencinaCode/Character/ValencinaRestSiteCharacterNode.cs")]
public class ValencinaRestSiteCharacterNode : NRestSiteCharacter
{
	public class MethodName : MethodName
	{
		public static readonly StringName _Ready = StringName.op_Implicit("_Ready");

		public static readonly StringName ConnectRestSiteHitbox = StringName.op_Implicit("ConnectRestSiteHitbox");
	}

	public class PropertyName : PropertyName
	{
	}

	public class SignalName : SignalName
	{
	}

	private static readonly FieldRef<NRestSiteCharacter, Control> ControlRootRef = AccessTools.FieldRefAccess<NRestSiteCharacter, Control>("_controlRoot");

	private static readonly FieldRef<NRestSiteCharacter, NSelectionReticle> SelectionReticleRef = AccessTools.FieldRefAccess<NRestSiteCharacter, NSelectionReticle>("_selectionReticle");

	private static readonly FieldRef<NRestSiteCharacter, Control> LeftThoughtAnchorRef = AccessTools.FieldRefAccess<NRestSiteCharacter, Control>("_leftThoughtAnchor");

	private static readonly FieldRef<NRestSiteCharacter, Control> RightThoughtAnchorRef = AccessTools.FieldRefAccess<NRestSiteCharacter, Control>("_rightThoughtAnchor");

	private static readonly FieldRef<NRestSiteCharacter, Control> HitboxRef = AccessTools.FieldRefAccess<NRestSiteCharacter, Control>("<Hitbox>k__BackingField");

	public override void _Ready()
	{
		ControlRootRef.Invoke((NRestSiteCharacter)(object)this) = ((Node)this).GetNode<Control>(NodePath.op_Implicit("ControlRoot"));
		HitboxRef.Invoke((NRestSiteCharacter)(object)this) = ((Node)this).GetNode<Control>(NodePath.op_Implicit("%Hitbox"));
		SelectionReticleRef.Invoke((NRestSiteCharacter)(object)this) = ((Node)this).GetNode<NSelectionReticle>(NodePath.op_Implicit("%SelectionReticle"));
		LeftThoughtAnchorRef.Invoke((NRestSiteCharacter)(object)this) = ((Node)this).GetNode<Control>(NodePath.op_Implicit("%ThoughtBubbleLeft"));
		RightThoughtAnchorRef.Invoke((NRestSiteCharacter)(object)this) = ((Node)this).GetNode<Control>(NodePath.op_Implicit("%ThoughtBubbleRight"));
		ConnectRestSiteHitbox();
		ValencinaRestSiteGlow.AddTo((NRestSiteCharacter)(object)this);
		MainFile.Logger.Info("[ValencinaRestSite] Custom rest-site _Ready complete; vanilla ActIndex switch bypassed.", 1);
	}

	private void ConnectRestSiteHitbox()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		Action action = AccessTools.MethodDelegate<Action>(AccessTools.Method(typeof(NRestSiteCharacter), "OnFocus", (Type[])null, (Type[])null), (object)this, true, (Type[])null);
		Action action2 = AccessTools.MethodDelegate<Action>(AccessTools.Method(typeof(NRestSiteCharacter), "OnUnfocus", (Type[])null, (Type[])null), (object)this, true, (Type[])null);
		((GodotObject)((NRestSiteCharacter)this).Hitbox).Connect(SignalName.FocusEntered, Callable.From(action), 0u);
		((GodotObject)((NRestSiteCharacter)this).Hitbox).Connect(SignalName.FocusExited, Callable.From(action2), 0u);
		((GodotObject)((NRestSiteCharacter)this).Hitbox).Connect(SignalName.MouseEntered, Callable.From(action), 0u);
		((GodotObject)((NRestSiteCharacter)this).Hitbox).Connect(SignalName.MouseExited, Callable.From(action2), 0u);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		return new List<MethodInfo>(2)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.ConnectRestSiteHitbox, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if ((ref method) == MethodName._Ready && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			((Node)this)._Ready();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.ConnectRestSiteHitbox && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			ConnectRestSiteHitbox();
			ret = default(godot_variant);
			return true;
		}
		return ((NRestSiteCharacter)this).InvokeGodotClassMethod(ref method, args, ref ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if ((ref method) == MethodName._Ready)
		{
			return true;
		}
		if ((ref method) == MethodName.ConnectRestSiteHitbox)
		{
			return true;
		}
		return ((NRestSiteCharacter)this).HasGodotClassMethod(ref method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		((NRestSiteCharacter)this).SaveGodotObjectData(info);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		((NRestSiteCharacter)this).RestoreGodotObjectData(info);
	}
}
