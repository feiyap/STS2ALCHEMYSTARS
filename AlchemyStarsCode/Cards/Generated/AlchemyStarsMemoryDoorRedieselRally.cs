using AlchemyStars.Events;
using AlchemyStars.Keywords;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>万门之钥选项：红油拉力赛（水）。</summary>
[RegisterCard(typeof(StatusCardPool))]
public sealed class AlchemyStarsMemoryDoorRedieselRally : AlchemyStarsMemoryDoorChoiceCardBase
{
    public override Type TargetEventType => typeof(AlchemyStarsRedieselRally);

    protected override IReadOnlyList<string> AttributeKeywordIds => [AlchemyStarsKeywordIds.Water];

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/AlchemyStarsMemoryDoorRedieselRally.png");
}
