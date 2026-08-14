using AlchemyStars.Keywords;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Cards;

/// <summary>
/// 木月白：启迪者属性选择展示牌（水）。
/// </summary>
[RegisterCard(typeof(StatusCardPool))]
public sealed class AlchemyStarsEnlightenerChoiceMuYuebai : AlchemyStarsEnlightenerChoiceCardBase
{
    protected override string AttributeKeywordId => AlchemyStarsKeywordIds.Water;
}
