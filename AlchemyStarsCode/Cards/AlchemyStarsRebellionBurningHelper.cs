using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using AlchemyStars.Keywords;
using STS2RitsuLib.Keywords;

namespace AlchemyStars.Cards;

/// <summary>
/// ������ȼ�����������⡣
/// </summary>
internal static class AlchemyStarsRebellionBurningHelper
{
    private static readonly HashSet<Type> AwakenedRebelCardTypes =
    [
        typeof(AlchemyStarsThunderUncommon3),
        typeof(AlchemyStarsThunderUncommon4),
        typeof(AlchemyStarsThunderRare3)
    ];

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
