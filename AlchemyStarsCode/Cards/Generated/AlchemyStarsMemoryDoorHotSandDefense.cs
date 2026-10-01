using AlchemyStars.Events;
using AlchemyStars.Keywords;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>万门之钥选项：热砂攻防战（火/雷）。</summary>
[RegisterCard(typeof(StatusCardPool))]
public sealed class AlchemyStarsMemoryDoorHotSandDefense : AlchemyStarsMemoryDoorChoiceCardBase
{
    public override Type TargetEventType => typeof(AlchemyStarsHotSandDefense);

    protected override IReadOnlyList<string> AttributeKeywordIds =>
    [
        AlchemyStarsKeywordIds.Fire,
        AlchemyStarsKeywordIds.Thunder,
    ];

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/AlchemyStarsMemoryDoorHotSandDefense.png");
}
