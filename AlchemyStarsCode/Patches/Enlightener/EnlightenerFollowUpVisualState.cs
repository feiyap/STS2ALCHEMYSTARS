using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Models;

namespace AlchemyStars.Patches.Enlightener;

/// <summary>
/// 记录当前先古事件房间应显示的启迪者视觉 entry。
/// </summary>
internal static class EnlightenerFollowUpVisualState
{
    private sealed class Entry(string eventEntry)
    {
        internal string EventEntry { get; } = eventEntry;
    }

    private static readonly ConditionalWeakTable<AncientEventModel, Entry> Entries = new();

    internal static void Set(AncientEventModel hostEvent, string eventEntry)
    {
        Entries.Remove(hostEvent);
        Entries.Add(hostEvent, new Entry(eventEntry));
    }

    internal static bool TryGet(EventModel eventModel, out string eventEntry)
    {
        if (eventModel is AncientEventModel ancient && Entries.TryGetValue(ancient, out var entry))
        {
            eventEntry = entry.EventEntry;
            return true;
        }

        eventEntry = string.Empty;
        return false;
    }
}
