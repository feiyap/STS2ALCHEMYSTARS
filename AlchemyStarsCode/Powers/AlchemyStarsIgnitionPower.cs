using System.Collections.Generic;
using System.Threading.Tasks;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 灼燃：增强下次火属性伤害 20%（灼灼海棠下额外 +20%），并消耗 1 层。
/// 同一张牌的多段伤害只消耗 1 层，避免约拿等多段攻击一次烧光。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsIgnitionPower : AlchemyStarsPowerBase
{
    public const decimal BaseBonusRate = 0.2m;
    public const decimal BegoniaExtraRate = 0.2m;

    private CardModel? _consumedForCard;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

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

        // 同一卡牌来源的多段火伤只消耗一层。
        if (cardSource != null && ReferenceEquals(cardSource, _consumedForCard))
            return;

        Flash();
        _consumedForCard = cardSource;
        await PowerCmd.Decrement(this);
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature == Owner)
            _consumedForCard = null;
        return Task.CompletedTask;
    }
}
