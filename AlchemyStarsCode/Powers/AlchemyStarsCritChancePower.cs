using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Powers;

/// <summary>
/// 暴击充能：层数是暴击几率（百分比）。实际结算的攻击有相应几率造成 2 倍伤害。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsCritChancePower : AlchemyStarsPowerBase
{
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
        if (dealer != Owner || Amount <= 0 || !props.IsPoweredAttack())
            return 1m;

        // 悬停预览没有本次打出，也不在属性伤害上下文里，不在这里结算暴击。
        var resolvingHit = cardPlay != null
            || LightMechanicDamageContext.CurrentElement != null
            || LightMechanicDamageContext.IsFireAndThunder;
        if (!resolvingHit)
            return 1m;

        // 超过 100% 必定暴击，超出的每 1% 让暴击倍率 +0.01（150% 为 2.5 倍）。
        if (Amount > 100m)
            return 2m + (Amount - 100m) / 100m;

        if (Amount >= 100m)
            return 2m;

        var player = Owner.Player;
        if (player == null)
            return 1m;

        var roll = player.RunState.Rng.Niche.NextInt(100);
        return roll < (int)Amount ? 2m : 1m;
    }
}
