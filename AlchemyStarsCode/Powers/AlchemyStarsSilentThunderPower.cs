using System.Threading.Tasks;
using AlchemyStars.Mechanics;
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
/// 静声之雷：受到伤害时，额外受到最大生命值 0.5% × 层数的伤害（不消耗层数）。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsSilentThunderPower : ModPowerTemplate
{
    private const decimal MaxHpBonusPercentPerStack = 0.005m;

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner || Amount <= 0 || result.UnblockedDamage <= 0m)
            return;

        // 避免自身额外伤害递归触发。
        if (props.HasFlag(ValueProp.Unpowered))
            return;

        Flash();

        var bonus = Owner.MaxHp * MaxHpBonusPercentPerStack * Amount;
        if (bonus <= 0m)
            return;

        using (LightMechanicDamageContext.Use(LightElement.Thunder))
        {
            await CreatureCmd.Damage(
                choiceContext,
                Owner,
                bonus,
                ValueProp.Unblockable | ValueProp.Unpowered,
                cardSource,
                null);
        }
    }
}
