using AlchemyStars.Characters;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Enlightener;

/// <summary>
/// 光能追踪方案 C：每拾取属性卡，该属性在奖励/商店中的出现权重 +15%。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsLightTrackingPlanC : AlchemyStarsEnlightenerRelicBase
{
    private int _firePickups;
    private int _waterPickups;
    private int _thunderPickups;
    private int _forestPickups;

    [SavedProperty]
    public int FirePickups
    {
        get => _firePickups;
        set
        {
            AssertMutable();
            _firePickups = value;
        }
    }

    [SavedProperty]
    public int WaterPickups
    {
        get => _waterPickups;
        set
        {
            AssertMutable();
            _waterPickups = value;
        }
    }

    [SavedProperty]
    public int ThunderPickups
    {
        get => _thunderPickups;
        set
        {
            AssertMutable();
            _thunderPickups = value;
        }
    }

    [SavedProperty]
    public int ForestPickups
    {
        get => _forestPickups;
        set
        {
            AssertMutable();
            _forestPickups = value;
        }
    }

    public override bool TryModifyCardBeingAddedToDeck(CardModel card, out CardModel? newCard)
    {
        newCard = null;
        if (card.Owner != Owner)
            return false;

        var attribute = AttributeCardTracking.TryGetCardAttribute(card);
        if (attribute == null)
            return false;

        RecordPickup(attribute.Value);
        return false;
    }

    public override bool TryModifyCardRewardOptionsLate(
        Player player,
        List<CardCreationResult> cardRewards,
        CardCreationOptions options)
    {
        if (player != Owner)
            return false;

        if (options.Flags.HasFlag(CardCreationFlags.NoModifyHooks))
            return false;

        // 尚无权重加成时不必重抽，避免无意义地扰动奖励。
        if (!HasWeightBonus)
            return false;

        var candidates = options.GetPossibleCards(player).ToList();
        if (candidates.Count == 0)
            return false;

        var rng = options.RngOverride ?? player.PlayerRng.Rewards;
        var modified = false;
        var blacklist = new HashSet<ModelId>();

        foreach (var reward in cardRewards)
        {
            var filtered = candidates.Where(card => !blacklist.Contains(card.Id)).ToList();
            if (RerollCard(reward, filtered, player, rng, matchType: false))
            {
                blacklist.Add(reward.Card.CanonicalInstance.Id);
                modified = true;
            }
            else
            {
                blacklist.Add(reward.Card.CanonicalInstance.Id);
            }
        }

        return modified;
    }

    public override void ModifyMerchantCardCreationResults(Player player, List<CardCreationResult> cards)
    {
        if (player != Owner || !HasWeightBonus)
            return;

        var pool = AttributeCardTracking.GetCharacterPoolCards(player)
            .Where(card => card.Rarity != CardRarity.Basic)
            .ToList();
        if (pool.Count == 0)
            return;

        var rng = player.PlayerRng.Shops;
        foreach (var entry in cards)
        {
            // 购物后会对整页货架再跑一遍本 Hook；已处理过则跳过，避免整页卡牌被重抽刷新。
            if (entry.ModifyingRelics.Contains(this))
                continue;

            if (!RerollCard(entry, pool, player, rng, matchType: true))
            {
                // 权重仍抽中原卡时也打标，防止后续 UpdateEntry 再次重抽。
                entry.ModifyCard(entry.Card, this);
            }
        }
    }

    private bool HasWeightBonus =>
        FirePickups > 0 || WaterPickups > 0 || ThunderPickups > 0 || ForestPickups > 0;

    private bool RerollCard(
        CardCreationResult result,
        IReadOnlyList<CardModel> pool,
        Player player,
        Rng rng,
        bool matchType)
    {
        var current = result.Card;
        IEnumerable<CardModel> filtered = pool.Where(card => card.Rarity == current.Rarity);
        if (matchType)
            filtered = filtered.Where(card => card.Type == current.Type);

        // 排除已出现在同批奖励中的其他卡（按 canonical），降低重复。
        var list = filtered.ToList();
        if (list.Count == 0)
            return false;

        var picked = AttributeCardTracking.WeightedPick(list, GetPickupCount, rng);
        if (picked == null || picked.Id == current.CanonicalInstance.Id)
            return false;

        var wasUpgraded = current.IsUpgraded;
        var newCard = player.RunState.CreateCard(picked, player);
        if (wasUpgraded && newCard.IsUpgradable)
            CardCmd.Upgrade(newCard);

        result.ModifyCard(newCard, this);
        return true;
    }

    private void RecordPickup(LightElement attribute)
    {
        switch (attribute)
        {
            case LightElement.Fire:
                FirePickups++;
                break;
            case LightElement.Water:
                WaterPickups++;
                break;
            case LightElement.Thunder:
                ThunderPickups++;
                break;
            case LightElement.Forest:
                ForestPickups++;
                break;
        }
    }

    private int GetPickupCount(LightElement attribute) => attribute switch
    {
        LightElement.Fire => FirePickups,
        LightElement.Water => WaterPickups,
        LightElement.Thunder => ThunderPickups,
        LightElement.Forest => ForestPickups,
        _ => 0,
    };
}
