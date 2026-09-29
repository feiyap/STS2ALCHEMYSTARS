using System.Linq;
using AlchemyStars.Events;
using Godot;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Events;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Saves;
using STS2RitsuLib.Content;
using STS2RitsuLib.Localization;

namespace AlchemyStars.Patches.Enlightener;

/// <summary>
/// 将事件房间标题、对话与先古舞台切换为启迪者。
/// </summary>
internal static class EnlightenerFollowUpVisuals
{
    private const string PortraitNodeName = "AlchemyStarsEnlightenerPortrait";

    internal static void Apply(string eventEntry, AncientEventModel? hostEvent = null)
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

            if (hostEvent?.Owner != null && room.Layout is NAncientEventLayout ancientLayout)
                ApplyDialogue(ancientLayout, hostEvent);
        }
        catch (Exception ex)
        {
            Entry.Logger.Warn($"[Enlightener] 应用视觉失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 涅奥续页宿主仍是 Neow，需手动灌入启迪者对话集。
    /// </summary>
    private static void ApplyDialogue(NAncientEventLayout layout, AncientEventModel hostEvent)
    {
        var owner = hostEvent.Owner;
        if (owner?.Character == null)
            return;

        const string dialogueEntry = AlchemyStarsEnlightener.DialogueEntry;
        var dialogueSet = AncientDialogueLocalization.BuildDialogueSetForModAncient(dialogueEntry);
        // 仅注入当前角色对话（RitsuLib 无 GetModCharacters API）。
        AncientDialogueLocalization.AppendCharacterDialogues(
            dialogueSet,
            dialogueEntry,
            [owner.Character]);
        dialogueSet.PopulateLocKeys(dialogueEntry);

        // 启迪者不在地图先古池，Progress 可能无统计；缺省按第 0 次相遇播对话。
        var stats = SaveManager.Instance.Progress.GetStatsForAncient(hostEvent.Id);
        var charVisits = stats?.GetVisitsAs(owner.Character.Id) ?? 0;
        var totalVisits = stats?.TotalVisits ?? 0;

        var valid = dialogueSet
            .GetValidDialogues(
                owner.Character.Id,
                charVisits,
                totalVisits,
                allowAnyCharacterDialogues: true)
            .ToList();
        if (valid.Count == 0)
        {
            Entry.Logger.Warn("[Enlightener] 未找到可播放的启迪者对话。");
            return;
        }

        var picked = Rng.Chaotic.NextItem(valid);
        if (picked == null || picked.Lines.Count == 0)
            return;

        foreach (var line in picked.Lines)
            line.LineText?.Add("Act1Name", owner.RunState.Acts[0].Title);

        layout.ClearDialogue();
        layout.SetDialogue(picked.Lines);
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
