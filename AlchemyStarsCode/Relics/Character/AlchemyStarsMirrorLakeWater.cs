using System.Collections.Generic;
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
using MegaCrit.Sts2.Core.Nodes.CommonUi;
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

        // FromSerializable 不会绑定 Owner。未进 RunState 的牌无法加入牌组，回忆会失败，遗物就只剩删牌。
        var loaded = ForgottenCards
            .Select(save => Owner.RunState.LoadCard(save, Owner))
            .ToList();

        var prefs = new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, 1, loaded.Count)
        {
            Cancelable = true,
        };

        List<CardModel> picked;
        try
        {
            picked = (await CardSelectCmd.FromSimpleGrid(
                choiceContext,
                loaded,
                Owner,
                prefs)).ToList();
        }
        catch (OperationCanceledException)
        {
            // 选牌界面关闭时任务会被取消。把临时载入的牌移出当局，遗忘列表保持不变。
            foreach (var card in loaded)
                Owner.RunState.RemoveCard(card);
            return false;
        }
        catch
        {
            foreach (var card in loaded)
                Owner.RunState.RemoveCard(card);
            throw;
        }

        if (picked.Count == 0)
        {
            foreach (var card in loaded)
                Owner.RunState.RemoveCard(card);
            return false;
        }

        var pickedSet = new HashSet<CardModel>(picked, ReferenceEqualityComparer.Instance);
        var remaining = new List<SerializableCard>();
        for (var i = 0; i < loaded.Count; i++)
        {
            var card = loaded[i];
            if (!pickedSet.Contains(card))
            {
                remaining.Add(ForgottenCards[i]);
                Owner.RunState.RemoveCard(card);
                continue;
            }

            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
            if (card.IsUpgradable)
                CardCmd.Upgrade(card, CardPreviewStyle.MessyLayout);
        }

        ForgottenCards = remaining;
        Flash();
        return true;
    }
}
