using AlchemyStars.Characters;
using AlchemyStars.Enchantments;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 隐士典籍：3 场战斗后提供普通/罕见/稀有三组自选奖励；选中牌升级且费用永久为 0。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsHermitsTome : AlchemyStarsAncientRelicBase
{
    private const int CombatThreshold = 3;

    private int _combatsSeen;
    private bool _rewardOffered;
    private bool _modifyingCustomRewards;

    public override bool ShowCounter => !_rewardOffered;

    public override int DisplayAmount => Math.Min(CombatsSeen, CombatThreshold);

    [SavedProperty]
    public int CombatsSeen
    {
        get => _combatsSeen;
        set
        {
            AssertMutable();
            _combatsSeen = value;
            InvokeDisplayAmountChanged();
        }
    }

    [SavedProperty]
    public bool RewardOffered
    {
        get => _rewardOffered;
        set
        {
            AssertMutable();
            _rewardOffered = value;
            InvokeDisplayAmountChanged();
        }
    }

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        if (Owner == null || RewardOffered)
            return;

        CombatsSeen++;
        if (CombatsSeen < CombatThreshold)
            return;

        RewardOffered = true;
        Flash();

        _modifyingCustomRewards = true;
        try
        {
            await RewardsCmd.OfferCustom(Owner,
            [
                CreateRarityReward(CardRarity.Common),
                CreateRarityReward(CardRarity.Uncommon),
                CreateRarityReward(CardRarity.Rare),
            ]);
        }
        finally
        {
            _modifyingCustomRewards = false;
        }
    }

    public override bool TryModifyCardRewardOptions(
        MegaCrit.Sts2.Core.Entities.Players.Player player,
        List<CardCreationResult> cardRewardOptions,
        CardCreationOptions creationOptions)
    {
        if (!_modifyingCustomRewards || player != Owner)
            return false;

        foreach (var option in cardRewardOptions)
        {
            var card = option.Card;
            if (card.IsUpgradable)
                CardCmd.Upgrade(card);

            // 费用写在附魔上，读档时 OnEnchant 会再次把基础费用设为 0。
            // SetCustomBaseCost 本身不会进存档，重启后会回到卡牌原费用。
            if (!card.EnergyCost.CostsX && card.Enchantment == null)
                CardCmd.Enchant<AlchemyStarsHermitsTomeEnchantment>(card, 1m);
            else if (!card.EnergyCost.CostsX)
                card.EnergyCost.SetCustomBaseCost(0);
        }

        return true;
    }

    private CardReward CreateRarityReward(CardRarity rarity)
    {
        var options = new CardCreationOptions(
            [Owner!.Character.CardPool],
            CardCreationSource.Other,
            CardRarityOddsType.Uniform,
            c => c.Rarity == rarity);
        return new CardReward(options, 3, Owner);
    }
}
