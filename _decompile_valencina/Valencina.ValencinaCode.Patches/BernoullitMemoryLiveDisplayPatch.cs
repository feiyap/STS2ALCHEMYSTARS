using System;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Relics;
using Valencina.ValencinaCode.Relics;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NRelicInventoryHolder), "OnDisplayAmountChanged")]
internal static class BernoullitMemoryLiveDisplayPatch
{
	private static readonly PropertyInfo? IsFocusedProperty = AccessTools.Property(typeof(NClickableControl), "IsFocused");

	private static void Postfix(NRelicInventoryHolder __instance)
	{
		try
		{
			NRelic relic = __instance.Relic;
			if (!(((relic != null) ? relic.Model : null) is BernoullitMemory))
			{
				return;
			}
			object obj = IsFocusedProperty?.GetValue(__instance);
			if (obj is bool && (bool)obj)
			{
				NHoverTipSet.Remove((Control)(object)__instance);
				NHoverTipSet obj2 = NHoverTipSet.CreateAndShow((Control)(object)__instance, __instance.Relic.Model.HoverTips, (HoverTipAlignment)0);
				if (obj2 != null)
				{
					obj2.SetAlignmentForRelic(__instance.Relic);
				}
			}
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[BernoullitMemory] Failed to refresh live counter hover display: " + ex.Message, 1);
		}
	}
}
