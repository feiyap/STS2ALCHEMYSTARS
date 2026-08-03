using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 梦魇荆棘：敌人身上每�?1 层减益，克娜莉对其最终伤害增�?2%�?/// </summary>
[RegisterPower]
public sealed class AlchemyStarsNightmareThornPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (dealer != Owner || target == null || !props.IsPoweredAttack())
            return 1m;

        var debuffStacks = target.Powers
            .Where(power => power.Type == PowerType.Debuff && power.Amount > 0)
            .Sum(power => (int)power.Amount);

        return 1m + debuffStacks * 0.02m;
    }
}
