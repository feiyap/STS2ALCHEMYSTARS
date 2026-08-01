using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Events;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Valencina.ValencinaCode.Patches;

internal static class RienFollowUpAncientVisuals
{
	internal static void Apply(string eventEntry)
	{
		try
		{
			NEventRoom instance = NEventRoom.Instance;
			if (((instance != null) ? instance.Layout : null) == null)
			{
				MainFile.Logger.Info("[RienSecondAncient] Skipping follow-up Ancient visuals for " + eventEntry + ": event room layout is not ready.", 1);
				return;
			}
			string slug = eventEntry.ToLowerInvariant();
			ApplyVisualPart(eventEntry, "title", delegate
			{
				ApplyTitle(eventEntry);
			});
			RienFollowUpAncientDialogueIconState.ClearCurrentIcon();
			ApplyVisualPart(eventEntry, "background", delegate
			{
				ApplyBackground(slug);
			});
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[RienSecondAncient] Failed to apply follow-up Ancient visuals for " + eventEntry + ": " + ex.Message, 1);
		}
	}

	private static void ApplyVisualPart(string eventEntry, string partName, Action apply)
	{
		try
		{
			apply();
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn($"[RienSecondAncient] Failed to apply follow-up Ancient {partName} for {eventEntry}: {ex.Message}", 1);
		}
	}

	private static void ApplyTitle(string eventEntry)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		LocString val = new LocString("ancients", eventEntry + ".title");
		if (!val.Exists())
		{
			MainFile.Logger.Warn("[RienSecondAncient] Missing follow-up Ancient title loc: " + eventEntry + ".title", 1);
			return;
		}
		string formattedText = val.GetFormattedText();
		NEventRoom instance = NEventRoom.Instance;
		if (instance != null)
		{
			NEventLayout layout = instance.Layout;
			if (layout != null)
			{
				layout.SetTitle(formattedText);
			}
		}
		ApplyAncientNameBanner(eventEntry, formattedText);
	}

	private static void ApplyAncientNameBanner(string eventEntry, string formattedTitle)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		NEventRoom instance = NEventRoom.Instance;
		Node val = (Node)(object)((instance != null) ? instance.Layout : null);
		if (val == null)
		{
			return;
		}
		LocString val2 = new LocString("ancients", eventEntry + ".epithet");
		string text = (val2.Exists() ? val2.GetFormattedText() : null);
		foreach (Node item in FindChildren<Node>(val))
		{
			if (!((object)item).GetType().Name.Contains("AncientNameBanner", StringComparison.Ordinal))
			{
				continue;
			}
			bool flag = false;
			RichTextLabel nodeOrNull = item.GetNodeOrNull<RichTextLabel>(NodePath.op_Implicit("%Title"));
			if (nodeOrNull != null)
			{
				nodeOrNull.BbcodeEnabled = true;
				nodeOrNull.Text = "[ancient_banner]" + formattedTitle.ToUpperInvariant() + "[/ancient_banner]";
				flag = true;
			}
			Node nodeOrNull2 = item.GetNodeOrNull<Node>(NodePath.op_Implicit("%Epithet"));
			if (text != null && nodeOrNull2 != null)
			{
				if (((GodotObject)nodeOrNull2).HasMethod(StringName.op_Implicit("SetTextAutoSize")))
				{
					((GodotObject)nodeOrNull2).Call(StringName.op_Implicit("SetTextAutoSize"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(text) });
				}
				else
				{
					Label val3 = (Label)(object)((nodeOrNull2 is Label) ? nodeOrNull2 : null);
					if (val3 != null)
					{
						val3.Text = text;
					}
				}
				flag = true;
			}
			if (flag)
			{
				MainFile.Logger.Info("[RienSecondAncient] Applied follow-up Ancient title banner for " + eventEntry + ".", 1);
				return;
			}
		}
		MainFile.Logger.Warn("[RienSecondAncient] Could not find Ancient name banner for " + eventEntry + ".", 1);
	}

	private static void ApplyPortrait(string slug)
	{
		string text = "res://Valencina/images/events/" + slug + ".png";
		if (!ResourceLoader.Exists(text, ""))
		{
			MainFile.Logger.Warn("[RienSecondAncient] Missing follow-up Ancient portrait: " + text, 1);
			return;
		}
		Texture2D val = ResourceLoader.Load<Texture2D>(text, (string)null, (CacheMode)1);
		NEventRoom instance = NEventRoom.Instance;
		object obj;
		if (instance == null)
		{
			obj = null;
		}
		else
		{
			NEventLayout layout = instance.Layout;
			obj = ((layout != null) ? ((Node)layout).GetNodeOrNull<Control>(NodePath.op_Implicit("%Portrait")) : null);
		}
		if (obj != null)
		{
			NEventRoom.Instance.SetPortrait(val);
			return;
		}
		RienFollowUpAncientDialogueIconState.SetCurrentIcon(text);
		ApplyDialogueIconToExistingLines(val);
	}

	private static void ApplyDialogueIconToExistingLines(Texture2D icon)
	{
		NEventRoom instance = NEventRoom.Instance;
		Node val = (Node)(object)((instance != null) ? instance.Layout : null);
		if (val == null)
		{
			return;
		}
		foreach (NAncientDialogueLine item in FindChildren<NAncientDialogueLine>(val))
		{
			RienFollowUpAncientDialogueIconState.TryApplyToLine(item, icon);
		}
	}

	private static void ApplyBackground(string slug)
	{
		NEventRoom instance = NEventRoom.Instance;
		object obj;
		if (instance == null)
		{
			obj = null;
		}
		else
		{
			NEventLayout layout = instance.Layout;
			obj = ((layout != null) ? ((Node)layout).GetNodeOrNull<Node>(NodePath.op_Implicit("%AncientBgContainer")) : null);
		}
		Node val = (Node)obj;
		if (val == null)
		{
			MainFile.Logger.Warn("[RienSecondAncient] Could not find %AncientBgContainer for follow-up Ancient background.", 1);
			return;
		}
		Node val2 = CreateBackgroundFromScene(slug) ?? CreateBackgroundFromTexture(slug);
		if (val2 == null)
		{
			MainFile.Logger.Warn("[RienSecondAncient] Missing follow-up Ancient background for " + slug + ".", 1);
			return;
		}
		foreach (Node child in val.GetChildren(false))
		{
			val.RemoveChild(child);
			child.QueueFree();
		}
		val.AddChild(val2, false, (InternalMode)0);
		MainFile.Logger.Info("[RienSecondAncient] Applied follow-up Ancient visuals for " + slug + ".", 1);
	}

	private static Node? CreateBackgroundFromScene(string slug)
	{
		string text = "res://scenes/events/background_scenes/" + slug + ".tscn";
		if (!ResourceLoader.Exists(text, ""))
		{
			return null;
		}
		return ResourceLoader.Load<PackedScene>(text, (string)null, (CacheMode)1).Instantiate((GenEditState)0);
	}

	private static Node? CreateBackgroundFromTexture(string slug)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected O, but got Unknown
		string text = "res://Valencina/images/events/" + slug + "_background.png";
		if (!ResourceLoader.Exists(text, ""))
		{
			return null;
		}
		return (Node?)new TextureRect
		{
			Name = StringName.op_Implicit(slug + "_background"),
			Texture = ResourceLoader.Load<Texture2D>(text, (string)null, (CacheMode)1),
			LayoutMode = 1,
			AnchorsPreset = 15,
			AnchorRight = 1f,
			AnchorBottom = 1f,
			GrowHorizontal = (GrowDirection)2,
			GrowVertical = (GrowDirection)2,
			ExpandMode = (ExpandModeEnum)1,
			StretchMode = (StretchModeEnum)6
		};
	}

	private static IEnumerable<T> FindChildren<T>(Node root) where T : Node
	{
		foreach (Node child in root.GetChildren(false))
		{
			T val = (T)(object)((child is T) ? child : null);
			if (val != null)
			{
				yield return val;
			}
			foreach (T item in FindChildren<T>(child))
			{
				yield return item;
			}
		}
	}
}
