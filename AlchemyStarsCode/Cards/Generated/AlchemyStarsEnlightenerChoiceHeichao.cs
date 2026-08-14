using AlchemyStars.Keywords;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Cards;

/// <summary>
/// 涡轮之声·黑潮：启迪者属性选择展示牌（火）。
/// </summary>
[RegisterCard(typeof(StatusCardPool))]
public sealed class AlchemyStarsEnlightenerChoiceHeichao : AlchemyStarsEnlightenerChoiceCardBase
{
    protected override string AttributeKeywordId => AlchemyStarsKeywordIds.Fire;
}
