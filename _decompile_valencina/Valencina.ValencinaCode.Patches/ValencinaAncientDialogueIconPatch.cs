using System;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Nodes.Events;
using Valencina.ValencinaCode.Character;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NAncientDialogueLine), "_Ready")]
internal static class ValencinaAncientDialogueIconPatch
{
	private const string ValencinaDialogueIconPath = "res://Valencina/images/ui/run_history/valencina_dialogue.png";

	private const string ValencinaDialogueOutlinePath = "res://Valencina/images/ui/run_history/valencina_dialogue_outline.png";

	private static readonly FieldInfo? LineField = AccessTools.Field(typeof(NAncientDialogueLine), "_line");

	private static readonly FieldInfo? CharacterField = AccessTools.Field(typeof(NAncientDialogueLine), "_character");

	private static void Postfix(NAncientDialogueLine __instance)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Invalid comparison between Unknown and I4
		try
		{
			RienFollowUpAncientDialogueIconState.TryApplyToLine(__instance);
			object? obj = LineField?.GetValue(__instance);
			AncientDialogueLine val = (AncientDialogueLine)((obj is AncientDialogueLine) ? obj : null);
			if (val == null || (int)val.Speaker != 2 || !(CharacterField?.GetValue(__instance) is Valencina.ValencinaCode.Character.Valencina))
			{
				return;
			}
			Texture2D val2 = ResourceLoader.Load<Texture2D>("res://Valencina/images/ui/run_history/valencina_dialogue.png", (string)null, (CacheMode)1);
			if (val2 != null)
			{
				Control node = ((Node)__instance).GetNode<Control>(NodePath.op_Implicit("%CharacterIcon"));
				((Node)node).GetNode<TextureRect>(NodePath.op_Implicit("Icon")).Texture = val2;
				Texture2D val3 = ResourceLoader.Load<Texture2D>("res://Valencina/images/ui/run_history/valencina_dialogue_outline.png", (string)null, (CacheMode)1);
				if (val3 != null)
				{
					((Node)node).GetNode<TextureRect>(NodePath.op_Implicit("Icon/Outline")).Texture = val3;
				}
			}
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[ValencinaAncientDialogueIcon] Failed to apply custom dialogue icon: " + ex.Message, 1);
		}
	}
}
