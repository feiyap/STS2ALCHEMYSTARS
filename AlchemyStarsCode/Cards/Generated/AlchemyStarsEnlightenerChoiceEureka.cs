using AlchemyStars.Keywords;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Cards;

/// <summary>
/// 尤莱雅：启迪者属性选择展示牌（森）。
/// </summary>
[RegisterCard(typeof(StatusCardPool))]
public sealed class AlchemyStarsEnlightenerChoiceEureka : AlchemyStarsEnlightenerChoiceCardBase
{
    protected override string AttributeKeywordId => AlchemyStarsKeywordIds.Forest;
}
