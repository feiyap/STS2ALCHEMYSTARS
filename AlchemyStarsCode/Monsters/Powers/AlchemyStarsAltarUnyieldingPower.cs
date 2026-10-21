using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Monsters.Powers;

/// <summary>
/// 不屈：与原版难以杀灭相同，单次受到的伤害不会超过层数。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsAltarUnyieldingPower : AlchemyStarsSwordAltarPowerBase
{
    protected override string PlaceholderIcon => "AlchemyStarsLockPower";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool ShouldScaleInMultiplayer => false;

    public override decimal ModifyDamageCap(
        Creature? target,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target != Owner || Amount <= 0)
            return decimal.MaxValue;

        return Amount;
    }

    public override Task AfterModifyingDamageAmount(CardModel? cardSource)
    {
        Flash();
        return Task.CompletedTask;
    }
}
