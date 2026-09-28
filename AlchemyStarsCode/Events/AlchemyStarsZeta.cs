using System.Collections.Generic;
using System.Linq;
using AlchemyStars.Relics.Ancients;
using Godot;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Events;

/// <summary>
/// 先古之民泽塔（第二层 / Hive）。
/// 运行时 Id.Entry 为 <c>ALCHEMY_STARS_EVENT_ALCHEMY_STARS_ZETA</c>；
/// ModAnalyzers 会误推为 ANCIENT 前缀，故 ancients.json 对两套 key 均保留。
/// </summary>
[RegisterActAncient(typeof(Hive))]
public sealed class AlchemyStarsZeta : ModAncientEventTemplate
{
    public const string PortraitPath = $"{Entry.ResPath}/images/events/AlchemyStarsZeta.png";

    public override Color ButtonColor => new(0.85f, 0.45f, 0.15f, 0.45f);
    public override Color DialogueColor => new("C46A2B");

    public override EventAssetProfile AssetProfile => new(InitialPortraitPath: PortraitPath);

    public override AncientEventPresentationAssetProfile AncientPresentationAssetProfile => new(
        StageProcedural: AncientEventStageProceduralVisualSetBuilder.Create()
            .Background(cues => cues.Single("loop", PortraitPath))
            .Build(),
        MapIconPath: PortraitPath,
        MapIconOutlinePath: PortraitPath,
        RunHistoryIconPath: PortraitPath,
        RunHistoryIconOutlinePath: PortraitPath);

    private IReadOnlyList<EventOption> PoolA =>
    [
        CreateModRelicOption<AlchemyStarsLogisticsLicense>(),
        CreateModRelicOption<AlchemyStarsDemonEye>(),
        CreateModRelicOption<AlchemyStarsWeirdCandy>(),
    ];

    private IReadOnlyList<EventOption> PoolB =>
    [
        CreateModRelicOption<AlchemyStarsZetaCandyJar>(),
        CreateModRelicOption<AlchemyStarsOddGameCard>(),
        CreateModRelicOption<AlchemyStarsSweetMicrophone>(),
    ];

    private IReadOnlyList<EventOption> PoolC =>
    [
        CreateModRelicOption<AlchemyStarsRoysRules>(),
        CreateModRelicOption<AlchemyStarsRoysReward>(),
        CreateModRelicOption<AlchemyStarsSmoothCompass>(),
        CreateModRelicOption<AlchemyStarsRallyTrophy>(),
    ];

    public override IEnumerable<EventOption> AllPossibleOptions =>
        PoolA.Concat(PoolB).Concat(PoolC);

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        Rng.NextItem(PoolA)!,
        Rng.NextItem(PoolB)!,
        Rng.NextItem(PoolC)!,
    ];
}
