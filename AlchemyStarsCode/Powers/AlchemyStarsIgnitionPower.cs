using System.Collections.Generic;
using System.Threading.Tasks;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using AlchemyStars.Keywords;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 灼燃：增强下次火属性伤害 20%（灼灼海棠下额外 +20%），并消耗 1 层。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsIgnitionPower : ModPowerTemplate
{
    public const decimal BaseBonusRate = 0.2m;
    public const decimal BegoniaExtraRate = 0.2m;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<string> RegisteredKeywordIds => [AlchemyStarsKeywordIds.Ignition];

    public static decimal GetBonusRate(MegaCrit.Sts2.Core.Entities.Players.Player player)
    {
        var rate = BaseBonusRate;
        if (player.Creature.GetPowerAmount<AlchemyStarsBloomingBegoniaPower>() > 0)
            rate += BegoniaExtraRate;
        return rate;
    }

    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        if (dealer != Owner || Amount <= 0)
            return;

        var element = LightMechanicDamageContext.CurrentElement;
        if (element is not (LightElement.Fire or LightElement.Prismatic))
            return;

        Flash();
        await PowerCmd.Decrement(this);
    }
}
