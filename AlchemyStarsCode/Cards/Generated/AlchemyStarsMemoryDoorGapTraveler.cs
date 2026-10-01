using AlchemyStars.Events;
using AlchemyStars.Keywords;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>万门之钥选项：隙间旅人（火）。</summary>
[RegisterCard(typeof(StatusCardPool))]
public sealed class AlchemyStarsMemoryDoorGapTraveler : AlchemyStarsMemoryDoorChoiceCardBase
{
    public override Type TargetEventType => typeof(AlchemyStarsGapTraveler);

    protected override IReadOnlyList<string> AttributeKeywordIds => [AlchemyStarsKeywordIds.Fire];

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/AlchemyStarsMemoryDoorGapTraveler.png");
}
