using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 哀伤之弦：每份使攻击伤害提升 50%。Amount = 份数。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsChloeDamagePower : AlchemyStarsPowerBase
{
    private const decimal DamageBonusPerCopy = 0.5m;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (dealer != Owner || !props.IsPoweredAttack())
            return 1m;

        var copies = Amount > 0m ? Amount : 1m;
        return 1m + copies * DamageBonusPerCopy;
    }
}
