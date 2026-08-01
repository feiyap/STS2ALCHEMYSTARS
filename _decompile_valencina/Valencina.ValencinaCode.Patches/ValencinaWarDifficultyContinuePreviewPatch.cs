using System;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.addons.mega_text;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
internal static class ValencinaWarDifficultyContinuePreviewPatch
{
	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(typeof(NContinueRunInfo), "ShowInfo", (Type[])null, (Type[])null);
	}

	private static void Postfix(NContinueRunInfo __instance, SerializableRun save)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (save.Ascension == 11)
		{
			MegaRichTextLabel nodeOrNull = ((Node)__instance).GetNodeOrNull<MegaRichTextLabel>(NodePath.op_Implicit("%AscensionLabel"));
			if (nodeOrNull != null)
			{
				string formattedText = new LocString("main_menu_ui", "CONTINUE_RUN_INFO.ascension").GetFormattedText();
				nodeOrNull.Text = formattedText + " ?";
				((CanvasItem)nodeOrNull).Visible = true;
			}
		}
	}
}
