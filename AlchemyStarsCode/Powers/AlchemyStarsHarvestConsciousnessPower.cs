using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 收割意识：每层按伤害的 10% 额外施加灾厄。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsHarvestConsciousnessPower : AlchemyStarsPowerBase
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (dealer != Owner || Amount <= 0 || target.IsDead)
            return;

        if (!props.IsPoweredAttack())
            return;

        var damageDealt = result.UnblockedDamage;
        if (damageDealt <= 0m)
            return;

        var calamity = (int)decimal.Floor(damageDealt * 0.1m * Amount);
        if (calamity <= 0)
            return;

        Flash();
        await PowerCmd.Apply<AlchemyStarsCalamityPower>(
            choiceContext,
            target,
            calamity,
            Owner,
            cardSource);
    }
}
