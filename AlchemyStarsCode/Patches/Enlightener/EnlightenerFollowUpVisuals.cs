using AlchemyStars.Events;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace AlchemyStars.Patches.Enlightener;

/// <summary>
/// 将事件房间标题切换为启迪者。
/// </summary>
internal static class EnlightenerFollowUpVisuals
{
    internal static void Apply(string eventEntry)
    {
        try
        {
            var room = NEventRoom.Instance;
            if (room?.Layout == null)
                return;

            var title = new LocString("ancients", $"{eventEntry}.title");
            if (!title.Exists())
                title = new LocString("ancients", $"{AlchemyStarsEnlightener.EventEntry}.title");

            if (!title.Exists())
                return;

            room.Layout.SetTitle(title.GetFormattedText());
        }
        catch (Exception ex)
        {
            Entry.Logger.Warn($"[Enlightener] 应用视觉失败: {ex.Message}");
        }
    }
}
