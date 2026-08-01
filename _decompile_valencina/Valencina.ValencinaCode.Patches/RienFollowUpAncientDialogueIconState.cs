using Godot;
using MegaCrit.Sts2.Core.Nodes.Events;
using Valencina.ValencinaCode.Settings;

namespace Valencina.ValencinaCode.Patches;

internal static class RienFollowUpAncientDialogueIconState
{
	private static string? CurrentIconPath { get; set; }

	internal static void SetCurrentIcon(string iconPath)
	{
		if (!ValencinaModConfig.EnableRienFollowUpAncient)
		{
			CurrentIconPath = null;
		}
		else
		{
			CurrentIconPath = iconPath;
		}
	}

	internal static void ClearCurrentIcon()
	{
		CurrentIconPath = null;
	}

	internal static void TryApplyToLine(NAncientDialogueLine line)
	{
	}

	internal static void TryApplyToLine(NAncientDialogueLine line, Texture2D icon)
	{
	}
}
