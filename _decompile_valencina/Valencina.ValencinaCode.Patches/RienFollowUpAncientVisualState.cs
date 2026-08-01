using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Settings;

namespace Valencina.ValencinaCode.Patches;

internal static class RienFollowUpAncientVisualState
{
	private sealed class Entry(string eventEntry)
	{
		internal string EventEntry { get; } = eventEntry;
	}

	private static readonly ConditionalWeakTable<AncientEventModel, Entry> Entries = new ConditionalWeakTable<AncientEventModel, Entry>();

	internal static void Set(AncientEventModel hostEvent, string eventEntry)
	{
		Entries.Remove(hostEvent);
		if (ValencinaModConfig.EnableRienFollowUpAncient)
		{
			Entries.Add(hostEvent, new Entry(eventEntry));
		}
	}

	internal static bool TryGet(EventModel eventModel, out string eventEntry)
	{
		if (!ValencinaModConfig.EnableRienFollowUpAncient)
		{
			eventEntry = string.Empty;
			return false;
		}
		AncientEventModel val = (AncientEventModel)(object)((eventModel is AncientEventModel) ? eventModel : null);
		if (val != null && Entries.TryGetValue(val, out Entry value))
		{
			eventEntry = value.EventEntry;
			return true;
		}
		eventEntry = string.Empty;
		return false;
	}
}
