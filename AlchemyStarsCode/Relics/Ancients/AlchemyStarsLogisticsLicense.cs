using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 物流执照：商店价格 6 折；第 7 回合起每回合失去 10 金币。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsLogisticsLicense : AlchemyStarsAncientRelicBase
{
    private const decimal DiscountMultiplier = 0.6m;
    private const int GoldLossStartTurn = 7;
    private const int GoldLossPerTurn = 10;

    public override decimal ModifyMerchantPrice(Player player, MerchantEntry entry, decimal originalPrice)
    {
        if (player != Owner || !LocalContext.IsMe(Owner))
            return originalPrice;

        return originalPrice * DiscountMultiplier;
    }

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (Owner == null || !participants.Contains(Owner.Creature))
            return;

        if (Owner.PlayerCombatState.TurnNumber < GoldLossStartTurn)
            return;

        Flash();
        // LoseGold(amount, player)：金额在前，玩家在后。
        await PlayerCmd.LoseGold(GoldLossPerTurn, Owner);
    }
}
