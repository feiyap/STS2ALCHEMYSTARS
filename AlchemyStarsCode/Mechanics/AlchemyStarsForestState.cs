using System.Linq;
using AlchemyStars.Keywords;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Utils;

namespace AlchemyStars.Mechanics;

/// <summary>
/// 森系卡牌战斗内跨卡状态追踪�?/// </summary>
public static class AlchemyStarsForestState
{
    private static readonly AttachedState<Player, int> RetainEffectCount = new(_ => 0);
    private static readonly AttachedState<CardModel, int> PastRuptureBonus = new(_ => 0);
    private static readonly AttachedState<CardModel, int> RadiantBloomPlays = new(_ => 0);
    private static readonly AttachedState<Player, int> TeaPartyCooldown = new(_ => 0);
    private static readonly AttachedState<CardModel, int> KushkutaCombatDamageBonus = new(_ => 0);
    private static readonly AttachedState<CardModel, int> JenoRetainCount = new(_ => 0);
    private static readonly AttachedState<CardModel, int> ShinopuEnhanceUses = new(_ => 0);
    private static readonly AttachedState<CardModel, int> ReceiptMailHandSize = new(_ => 0);
    private static readonly AttachedState<CardModel, int> WordAbsoluteCostReduction = new(_ => 0);
    private static readonly AttachedState<CardModel, int> WordAbsoluteInitialCost = new(_ => 0);

    public static int GetRetainEffectCount(Player player) => RetainEffectCount[player];

    public static void IncrementRetainEffectCount(Player player) => RetainEffectCount[player]++;

    public static int GetPastRuptureBonus(CardModel card) => PastRuptureBonus[card];

    public static void IncrementPastRuptureBonus(CardModel card) => PastRuptureBonus[card]++;

    public static int GetRadiantBloomPlays(CardModel card) => RadiantBloomPlays[card];

    public static void IncrementRadiantBloomPlays(CardModel card) => RadiantBloomPlays[card]++;

    public static int GetTeaPartyCooldown(Player player) => TeaPartyCooldown[player];

    public static void SetTeaPartyCooldown(Player player, int turns) => TeaPartyCooldown[player] = turns;

    public static void TickTeaPartyCooldown(Player player)
    {
        if (TeaPartyCooldown[player] > 0)
            TeaPartyCooldown[player]--;
    }

    public static int GetKushkutaCombatDamageBonus(CardModel card) => KushkutaCombatDamageBonus[card];

    public static void IncrementKushkutaCombatDamageBonus(CardModel card, int amount = 1) =>
        KushkutaCombatDamageBonus[card] += amount;

    public static int GetJenoRetainCount(CardModel card) => JenoRetainCount[card];

    public static void IncrementJenoRetainCount(CardModel card) => JenoRetainCount[card]++;

    public static int GetShinopuEnhanceUses(CardModel card) => ShinopuEnhanceUses[card];

    public static void IncrementShinopuEnhanceUses(CardModel card) => ShinopuEnhanceUses[card]++;

    public static void ResetShinopuEnhanceUses(CardModel card) => ShinopuEnhanceUses[card] = 0;

    public static int GetReceiptMailHandSize(CardModel card) => ReceiptMailHandSize[card];

    public static void SetReceiptMailHandSize(CardModel card, int size) => ReceiptMailHandSize[card] = size;

    public static int GetWordAbsoluteCostReduction(CardModel card) => WordAbsoluteCostReduction[card];

    public static void IncrementWordAbsoluteCostReduction(CardModel card) => WordAbsoluteCostReduction[card]++;

    public static int GetWordAbsoluteInitialCost(CardModel card) => WordAbsoluteInitialCost[card];

    public static void SetWordAbsoluteInitialCost(CardModel card, int cost) => WordAbsoluteInitialCost[card] = cost;

    /// <summary>
    /// 转色栏产出森属性格时，触发手牌中「往昔溃裂」卡牌的伤害成长�?    /// </summary>
    public static void NotifyForestCellProduced(Player player)
    {
        var hand = player.PlayerCombatState?.Hand.Cards;
        if (hand == null)
            return;

        var pastRuptureKeyword = ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.PastRupture);
        foreach (var card in hand)
        {
            if (!card.Keywords.Contains(pastRuptureKeyword))
                continue;

            IncrementPastRuptureBonus(card);
            card.InvokeEnergyCostChanged();
        }
    }

    /// <summary>
    /// 获得强化格时，手牌中「言绝」卡牌吸收强化格并降低费用；返回实际吸收数量�?    /// </summary>
    public static int TryAbsorbEnhancedCellsForWordAbsolute(Player player, int cellsGained)
    {
        if (cellsGained <= 0)
            return 0;

        var hand = player.PlayerCombatState?.Hand.Cards;
        if (hand == null)
            return 0;

        var wordAbsoluteKeyword = ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.WordAbsolute);
        var wordAbsoluteCards = hand
            .Where(card => card.Keywords.Contains(wordAbsoluteKeyword))
            .ToList();

        if (wordAbsoluteCards.Count == 0)
            return 0;

        foreach (var card in wordAbsoluteCards)
        {
            IncrementWordAbsoluteCostReduction(card);
            card.InvokeEnergyCostChanged();
        }

        return cellsGained;
    }
}
