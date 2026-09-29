using Godot;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Events;

/// <summary>
/// 启迪者：仅作为涅奥后事件内续页的本地化/视觉模板，不进入地图先古池。
/// </summary>
public sealed class AlchemyStarsEnlightener : ModAncientEventTemplate
{
    public const string EventEntry = "ALCHEMY_STARS_ENLIGHTENER";
    /// <summary>对话本地化主键（与 ancients.json 中 EVENT 前缀一致）。</summary>
    public const string DialogueEntry = "ALCHEMY_STARS_EVENT_ALCHEMY_STARS_ENLIGHTENER";
    public const string PortraitPath = $"{Entry.ResPath}/images/events/AlchemyStarsEnlightener.png";

    public override Color DialogueColor => new("5BA3C9");

    public override LocString InitialDescription =>
        L10NLookup($"{EventEntry}.pages.INITIAL.description");

    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: PortraitPath);

    public override AncientEventPresentationAssetProfile AncientPresentationAssetProfile => new(
        StageProcedural: CreateStageVisuals());

    public override bool IsValidForAct(ActModel act) => false;

    public override bool IsAllowed(IRunState runState) => false;

    public override IEnumerable<EventOption> AllPossibleOptions => [];

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() => [];

    /// <summary>
    /// 用启迪者立绘铺满先古事件舞台。
    /// </summary>
    public static AncientEventStageProceduralVisualSet CreateStageVisuals() =>
        AncientEventStageProceduralVisualSetBuilder.Create()
            .Background(cues => cues.Single("loop", PortraitPath))
            .Build();
}
