using AlchemyStars.Keywords;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 木月白：启迪者属性选择展示牌（水）。
/// </summary>
[RegisterCard(typeof(StatusCardPool))]
public sealed class AlchemyStarsEnlightenerChoiceMuYuebai : AlchemyStarsEnlightenerChoiceCardBase
{
    protected override string AttributeKeywordId => AlchemyStarsKeywordIds.Water;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/AlchemyStarsEnlightenerChoiceMuYuebai.png");
}
