using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.sts2.Core.Nodes.TopBar;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NTopBarPortraitTip), "Initialize")]
internal static class ValencinaWarDifficultyRunHoverPatch
{
	private static readonly FieldInfo? HoverTipField = AccessTools.Field(typeof(NTopBarPortraitTip), "_hoverTip");

	private static void Postfix(NTopBarPortraitTip __instance, IRunState runState)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (ValencinaWarDifficulty.IsActive(runState) && !(HoverTipField == null))
		{
			HoverTipField.SetValue(__instance, (object)new HoverTip(new LocString("ascension", "LEVEL_11.hover"), (Texture2D)null));
		}
	}
}
