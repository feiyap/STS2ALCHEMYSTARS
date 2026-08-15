using System.Linq;
using AlchemyStars.Keywords;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Utils;

namespace AlchemyStars.Cards;

/// <summary>
/// 反叛灼燃：唤醒贡露、雷霆、莱因哈特，并在每回合第一次打出两张该词条牌时从抽牌堆自动打出一张。
/// </summary>
internal static class AlchemyStarsRebellionBurningHelper
{
    private const int EchoTriggerCount = 2;

    private static readonly HashSet<Type> AwakenedRebelCardTypes =
    [
        typeof(AlchemyStarsThunderUncommon3),
        typeof(AlchemyStarsThunderUncommon4),
        typeof(AlchemyStarsThunderRare3)
    ];

    private static readonly AttachedState<Player, int> PlaysThisTurn = new(_ => 0);
    private static readonly AttachedState<Player, bool> HasEchoedThisTurn = new(_ => false);

    private static CardKeyword RebellionBurningKeyword =>
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.RebellionBurning);

    public static bool IsAwakenedRebelCard(CardModel card) =>
        AwakenedRebelCardTypes.Contains(card.GetType());

    public static bool HasRebellionBurning(CardModel card) =>
        card.Keywords.Contains(RebellionBurningKeyword);

    public static void GrantRebellionBurningToAwakenedCards(Player player)
    {
        var keyword = RebellionBurningKeyword;
        foreach (var card in EnumerateOwnedCards(player))
        {
            if (!IsAwakenedRebelCard(card) || HasRebellionBurning(card))
                continue;

            card.AddKeyword(keyword);
        }
    }

    public static void ResetTurnTracking(Player player)
    {
        PlaysThisTurn[player] = 0;
        HasEchoedThisTurn[player] = false;
    }

    /// <summary>
    /// 本回合第 2 张反叛灼燃牌打出后，从抽牌堆随机自动打出一张同词条牌，每回合一次。
    /// </summary>
    public static async Task TryEchoAfterPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        var owner = card.Owner;
        if (owner == null || !HasRebellionBurning(card))
            return;

        PlaysThisTurn[owner]++;
        if (HasEchoedThisTurn[owner] || PlaysThisTurn[owner] != EchoTriggerCount)
            return;

        HasEchoedThisTurn[owner] = true;

        if (CombatManager.Instance.IsOverOrEnding || owner.Creature.IsDead)
            return;

        var drawPile = PileType.Draw.GetPile(owner);
        var candidates = drawPile.Cards
            .Where(candidate =>
                HasRebellionBurning(candidate) &&
                !candidate.Keywords.Contains(CardKeyword.Unplayable))
            .ToList();

        if (candidates.Count == 0)
            return;

        var picked = owner.RunState.Rng.CombatCardSelection.NextItem(candidates);
        if (picked == null)
            return;

        await CardPileCmd.Add(picked, PileType.Play);
        await CardCmd.AutoPlay(choiceContext, picked, null);
    }

    private static IEnumerable<CardModel> EnumerateOwnedCards(Player player)
    {
        foreach (var card in player.Deck.Cards)
            yield return card;

        var combat = player.PlayerCombatState;
        if (combat == null)
            yield break;

        foreach (var card in combat.AllCards)
            yield return card;
    }
}
