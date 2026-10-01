using System.Linq;
using AlchemyStars.Characters;
using AlchemyStars.RestSite;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Character;

/// <summary>
/// 镜湖之水：击败精英时可遗忘一张牌；休息处可回忆任意张并强化。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsMirrorLakeWater : AlchemyStarsCharacterRelicBase
{
    private List<SerializableCard> _forgotten = [];

    public override RelicRarity Rarity => RelicRarity.Rare;

    [SavedProperty]
    public List<SerializableCard> ForgottenCards
    {
        get => _forgotten;
        set
        {
            AssertMutable();
            _forgotten = value ?? [];
        }
    }

    public bool HasForgottenCards => ForgottenCards.Count > 0;

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        if (Owner == null || room.RoomType != RoomType.Elite)
            return;

        var deck = PileType.Deck.GetPile(Owner).Cards.Where(c => c != null).ToList();
        if (deck.Count == 0)
            return;

        Flash();
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 0, 1)
        {
            Cancelable = true,
        };
        var selected = (await CardSelectCmd.FromDeckGeneric(
            Owner,
            prefs,
            _ => true)).FirstOrDefault();
        if (selected == null)
            return;

        ForgottenCards = ForgottenCards.Append(selected.ToSerializable()).ToList();
        await CardPileCmd.RemoveFromDeck(selected);
    }

    public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
    {
        if (player != Owner || !HasForgottenCards)
            return false;

        return AlchemyStarsMirrorLakeRecallRestSiteOption.TryAddOption(player, options, this);
    }

    /// <summary>
    /// 回忆任意张被遗忘的牌：加入牌组并升级。
    /// </summary>
    public async Task<bool> RecallAsync(PlayerChoiceContext choiceContext)
    {
        if (Owner == null || ForgottenCards.Count == 0)
            return false;

        var previews = ForgottenCards
            .Select(CardModel.FromSerializable)
            .ToList();

        var prefs = new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, 1, previews.Count)
        {
            Cancelable = true,
        };
        var picked = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            previews,
            Owner,
            prefs)).ToList();
        if (picked.Count == 0)
            return false;

        var selectedIndices = new HashSet<int>();
        foreach (var card in picked)
        {
            var index = previews.IndexOf(card);
            if (index >= 0)
                selectedIndices.Add(index);
        }

        if (selectedIndices.Count == 0)
            return false;

        var remaining = new List<SerializableCard>();
        for (var i = 0; i < ForgottenCards.Count; i++)
        {
            if (!selectedIndices.Contains(i))
            {
                remaining.Add(ForgottenCards[i]);
                continue;
            }

            var card = CardModel.FromSerializable(ForgottenCards[i]);
            if (card.IsUpgradable)
                card.UpgradeInternal();

            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
        }

        ForgottenCards = remaining;
        Flash();
        return true;
    }
}
