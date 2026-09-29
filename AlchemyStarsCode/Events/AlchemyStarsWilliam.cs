using System.Collections.Generic;
using System.Linq;
using AlchemyStars.Characters;
using AlchemyStars.Relics.Ancients;
using Godot;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Events;

/// <summary>
/// 先古之民威廉（第三层 / Glory）。
/// 运行时 Id.Entry 为 <c>ALCHEMY_STARS_EVENT_ALCHEMY_STARS_WILLIAM</c>；
/// ModAnalyzers 会误推为 ANCIENT 前缀，故 ancients.json 对两套 key 均保留。
/// </summary>
[RegisterActAncient(typeof(Glory))]
public sealed class AlchemyStarsWilliam : ModAncientEventTemplate
{
    public const string PortraitPath = $"{Entry.ResPath}/images/events/AlchemyStarsWilliam.png";

    /// <summary>对话、顶栏与跑局记录用的先古头像。</summary>
    public const string SpeakerIconPath = $"{Entry.ResPath}/images/ancients/AlchemyStarsWilliamIcon.png";

    /// <summary>地图先古节点图标。描边已烘焙在贴图里。</summary>
    public const string MapIconPath = $"{Entry.ResPath}/images/ancients/AlchemyStarsWilliamMap.png";

    /// <summary>轮廓层留空，避免引擎再用章节色盖住已烘焙的主题色描边。</summary>
    private const string EmptyOutlinePath = $"{Entry.ResPath}/images/ancients/AlchemyStarsAncientEmptyOutline.png";

    public override Color ButtonColor => new(0.35f, 0.2f, 0.55f, 0.45f);
    public override Color DialogueColor => new("6B4C9A");

    public override EventAssetProfile AssetProfile => new(InitialPortraitPath: PortraitPath);

    public override AncientEventPresentationAssetProfile AncientPresentationAssetProfile => new(
        StageProcedural: AncientEventStageProceduralVisualSetBuilder.Create()
            .Background(cues => cues.Single("loop", PortraitPath))
            .Build(),
        MapIconPath: MapIconPath,
        MapIconOutlinePath: EmptyOutlinePath,
        RunHistoryIconPath: SpeakerIconPath,
        RunHistoryIconOutlinePath: EmptyOutlinePath);

    private IReadOnlyList<EventOption> PoolA =>
    [
        CreateModRelicOption<AlchemyStarsHermitsArcana>(),
        CreateModRelicOption<AlchemyStarsDropOfTruth>(),
        CreateModRelicOption<AlchemyStarsHermitsRite>(),
        CreateModRelicOption<AlchemyStarsLockOfObsession>(),
    ];

    private IReadOnlyList<EventOption> PoolB =>
    [
        CreateModRelicOption<AlchemyStarsHermitsTome>(),
        CreateModRelicOption<AlchemyStarsKeyOfManyDoors>(),
        CreateModRelicOption<AlchemyStarsFalseResurrection>(),
    ];

    private IReadOnlyList<EventOption> PoolC =>
    [
        CreateModRelicOption<AlchemyStarsEyeOfFate>(),
        CreateModRelicOption<AlchemyStarsHermitsVault>(),
        CreateModRelicOption<AlchemyStarsValueOfSoul>(),
    ];

    private IReadOnlyList<EventOption> PoolD =>
    [
        CreateModRelicOption<AlchemyStarsIsolatorsAppraisal>(),
        CreateModRelicOption<AlchemyStarsTruthLordsBlessing>(),
        CreateModRelicOption<AlchemyStarsNudgeTheFlow>(),
        CreateModRelicOption<AlchemyStarsMobiusOfSelf>(),
    ];

    public override IEnumerable<EventOption> AllPossibleOptions =>
        PoolA.Concat(PoolB).Concat(PoolC).Concat(PoolD);

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        var options = new List<EventOption>
        {
            Rng.NextItem(PoolA)!,
            Rng.NextItem(PoolB)!,
            Rng.NextItem(PoolC)!,
        };

        // 空裔专属第四选项。
        if (Owner?.Character is AlchemyStarsCharacter)
            options.Add(Rng.NextItem(PoolD)!);

        return options;
    }
}
