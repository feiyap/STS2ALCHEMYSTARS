using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using AlchemyStars.Mechanics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 麻痹：每层使受到的雷属性伤害增加 2%。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsParalysisPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target != Owner || Amount <= 0)
            return 1m;

        // 雷属性被动：仅抬高雷属性（含万色光能视为雷）伤害。
        var element = LightMechanicDamageContext.CurrentElement;
        if (element != LightElement.Thunder && element != LightElement.Prismatic)
            return 1m;

        return 1m + Amount * 0.02m;
    }
}
