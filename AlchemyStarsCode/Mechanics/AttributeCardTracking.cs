using AlchemyStars.Cards;
using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace AlchemyStars.Mechanics;

/// <summary>
/// 光能追踪方案共用的属性卡过滤与加权抽取。
/// </summary>
internal static class AttributeCardTracking
{
    public const float WeightBonusPerPickup = 0.15f;

    public static LightElement? TryGetCardAttribute(CardModel card)
    {
        if (AlchemyStarsCardHelpers.HasFireKeyword(card))
            return LightElement.Fire;
        if (AlchemyStarsCardHelpers.HasWaterKeyword(card))
            return LightElement.Water;
        if (AlchemyStarsCardHelpers.HasThunderKeyword(card))
            return LightElement.Thunder;
        if (AlchemyStarsCardHelpers.HasForestKeyword(card))
            return LightElement.Forest;
        return null;
    }

    public static bool MatchesAttribute(CardModel card, LightElement locked) =>
        TryGetCardAttribute(card) == locked;

    public static int ToAttributeMask(LightElement element) => 1 << (int)element;

    public static int ToAttributeMask(IEnumerable<LightElement> elements)
    {
        var mask = 0;
        foreach (var element in elements)
            mask |= ToAttributeMask(element);
        return mask;
    }

    public static bool IsAttributeLocked(LightElement element, int lockedMask) =>
        (lockedMask & ToAttributeMask(element)) != 0;

    /// <summary>
    /// 无属性卡一律放行；属性卡仅当属于任一已锁定属性时放行。
    /// </summary>
    public static bool PassesAttributeLock(CardModel card, int lockedMask)
    {
        if (lockedMask == 0)
            return true;

        var attribute = TryGetCardAttribute(card);
        if (attribute == null)
            return true;

        return IsAttributeLocked(attribute.Value, lockedMask);
    }

    /// <summary>
    /// 无属性卡一律放行；属性卡仅当匹配锁定属性时放行。
    /// </summary>
    public static bool PassesAttributeLock(CardModel card, LightElement locked) =>
        PassesAttributeLock(card, ToAttributeMask(locked));

    public static float GetAttributeWeight(CardModel card, Func<LightElement, int> getPickupCount)
    {
        var attribute = TryGetCardAttribute(card);
        if (attribute == null)
            return 1f;

        return 1f + WeightBonusPerPickup * getPickupCount(attribute.Value);
    }

    public static CardModel? WeightedPick(
        IEnumerable<CardModel> candidates,
        Func<LightElement, int> getPickupCount,
        Rng rng) =>
        rng.WeightedNextItem(candidates, card => GetAttributeWeight(card, getPickupCount));

    public static IEnumerable<CardModel> GetCharacterPoolCards(Player player) =>
        ModelDb.CardPool<AlchemyStarsCardPool>()
            .GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint);

    /// <summary>
    /// 将奖励中不符合锁定属性的属性卡替换为同稀有度的合法卡。
    /// </summary>
    public static bool RerollLockedRewardCards(
        Player player,
        List<CardCreationResult> cardRewards,
        CardCreationOptions options,
        int lockedMask,
        RelicModel modifyingRelic)
    {
        if (lockedMask == 0)
            return false;

        var pool = options.GetPossibleCards(player).ToList();
        if (pool.Count == 0)
            pool = GetCharacterPoolCards(player).ToList();

        var rng = options.RngOverride ?? player.PlayerRng.Rewards;
        var modified = false;
        var used = cardRewards
            .Select(r => r.Card.CanonicalInstance.Id)
            .ToHashSet();

        foreach (var reward in cardRewards)
        {
            var current = reward.Card;
            if (PassesAttributeLock(current, lockedMask))
                continue;

            var candidates = pool
                .Where(card =>
                    card.Rarity == current.Rarity &&
                    PassesAttributeLock(card, lockedMask) &&
                    !used.Contains(card.Id))
                .ToList();

            if (candidates.Count == 0)
            {
                candidates = pool
                    .Where(card =>
                        card.Rarity == current.Rarity &&
                        PassesAttributeLock(card, lockedMask))
                    .ToList();
            }

            if (candidates.Count == 0)
                continue;

            var picked = rng.NextItem(candidates);
            if (picked == null)
                continue;

            var wasUpgraded = current.IsUpgraded;
            var newCard = player.RunState.CreateCard(picked, player);
            if (wasUpgraded && newCard.IsUpgradable)
                CardCmd.Upgrade(newCard);

            used.Remove(current.CanonicalInstance.Id);
            used.Add(picked.Id);
            reward.ModifyCard(newCard, modifyingRelic);
            modified = true;
        }

        return modified;
    }
}
