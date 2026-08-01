using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(ImageHelper), "GetRoomIconPath")]
internal static class ValencinaRienRunHistoryIconSafePatch
{
	private static bool Prefix(MapPointType mapPointType, RoomType roomType, ModelId? modelId, ref string? __result)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)mapPointType != 8)
		{
			return true;
		}
		string iconPath = GetIconPath(modelId);
		if (iconPath == null)
		{
			return true;
		}
		__result = iconPath;
		return false;
	}

	internal static bool IsValencinaFollowUpAncient(ModelId? modelId)
	{
		return GetIconPath(modelId) != null;
	}

	private static string? GetIconPath(ModelId? modelId)
	{
		string entry = ((modelId != null) ? modelId.Entry : null);
		if (MatchesFollowUpEntry(entry, "THUMB_ADVISOR"))
		{
			return "res://Valencina/images/ui/run_history/thumb_advisor.png";
		}
		if (MatchesFollowUpEntry(entry, "LIMBUS_COMPANY_HEADQUARTERS"))
		{
			return "res://Valencina/images/ui/run_history/limbus_company_headquarters.png";
		}
		if (MatchesFollowUpEntry(entry, "RIEN"))
		{
			return "res://Valencina/images/ui/run_history/rien.png";
		}
		if (MatchesFollowUpEntry(entry, "STARS"))
		{
			return "res://Valencina/images/events/stars_background.webp";
		}
		return null;
	}

	private static bool MatchesFollowUpEntry(string? entry, string expected)
	{
		if (!string.IsNullOrWhiteSpace(entry))
		{
			if (!entry.Equals(expected, StringComparison.OrdinalIgnoreCase) && !entry.Equals("VALENCINA-" + expected, StringComparison.OrdinalIgnoreCase) && !entry.Equals("VALENCINA_" + expected, StringComparison.OrdinalIgnoreCase) && !entry.Equals("VALENCINASTS2-" + expected, StringComparison.OrdinalIgnoreCase) && !entry.Equals("VALENCINASTS2_" + expected, StringComparison.OrdinalIgnoreCase) && !entry.EndsWith("-" + expected, StringComparison.OrdinalIgnoreCase))
			{
				return entry.EndsWith("_" + expected, StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}
		return false;
	}
}
