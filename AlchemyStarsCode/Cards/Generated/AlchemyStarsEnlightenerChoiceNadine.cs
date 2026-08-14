using AlchemyStars.Keywords;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Cards;

/// <summary>
/// 长夜提灯·娜丁：启迪者属性选择展示牌（雷）。
/// </summary>
[RegisterCard(typeof(StatusCardPool))]
public sealed class AlchemyStarsEnlightenerChoiceNadine : AlchemyStarsEnlightenerChoiceCardBase
{
    protected override string AttributeKeywordId => AlchemyStarsKeywordIds.Thunder;
}
