using AlchemyStars.Events;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Events;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens;

namespace AlchemyStars.Patches.Enlightener;

/// <summary>
/// 将事件房间标题与先古舞台切换为启迪者。
/// </summary>
internal static class EnlightenerFollowUpVisuals
{
    private const string PortraitNodeName = "AlchemyStarsEnlightenerPortrait";

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

            if (title.Exists())
                room.Layout.SetTitle(title.GetFormattedText());

            ApplyPortrait(room.Layout);
        }
        catch (Exception ex)
        {
            Entry.Logger.Warn($"[Enlightener] 应用视觉失败: {ex.Message}");
        }
    }

    private static void ApplyPortrait(NEventLayout layout)
    {
        HideVanillaStage(layout);

        if (layout.GetNodeOrNull(PortraitNodeName) != null)
            return;

        if (!ResourceLoader.Exists(AlchemyStarsEnlightener.PortraitPath))
            return;

        var texture = ResourceLoader.Load<Texture2D>(AlchemyStarsEnlightener.PortraitPath);
        if (texture == null)
            return;

        var portrait = new TextureRect
        {
            Name = PortraitNodeName,
            Texture = texture,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            // 画在选项/标题之下，铺满事件布局（1920×1080 画布）。
            ZIndex = -50,
        };
        portrait.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        portrait.OffsetLeft = 0;
        portrait.OffsetTop = 0;
        portrait.OffsetRight = 0;
        portrait.OffsetBottom = 0;
        layout.AddChildSafely(portrait);
        layout.MoveChild(portrait, 0);
    }

    /// <summary>
    /// 原版先古背景容器在 16:9 下会缩到 0.89 并下移，不能铺满屏幕；启迪者立绘改挂在布局上后把它藏掉。
    /// </summary>
    private static void HideVanillaStage(NEventLayout layout)
    {
        if (layout is not NAncientEventLayout ancientLayout)
            return;

        var container = ancientLayout.GetNodeOrNull<NAncientBgContainer>("%AncientBgContainer");
        if (container == null || !GodotObject.IsInstanceValid(container))
            return;

        foreach (var child in container.GetChildren().ToList())
        {
            container.RemoveChildSafely(child);
            child.QueueFreeSafely();
        }

        container.Visible = false;
    }
}
