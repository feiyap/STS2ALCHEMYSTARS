using System.Linq;
using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Character;

/// <summary>
/// 十连召集：第一回合抽 10 选 4；每多抽 2 张可多留 1 张；固有牌不受影响。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsTenPullSummon : AlchemyStarsCharacterRelicBase
{
    private const int BaseDraw = 10;
    private const int BaseKeep = 4;

    private bool _pendingFirstTurn;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override Task BeforeCombatStart()
    {
        _pendingFirstTurn = true;
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom _)
    {
        _pendingFirstTurn = false;
        return Task.CompletedTask;
    }

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player != Owner || !_pendingFirstTurn)
            return count;
        return BaseDraw;
    }

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner || !_pendingFirstTurn || Owner == null)
            return;

        _pendingFirstTurn = false;
        var hand = PileType.Hand.GetPile(Owner);
        var selectables = hand.Cards.Where(c => !IsInnateCard(c)).ToList();
        if (selectables.Count <= BaseKeep)
            return;

        // 每超出基础 10 抽 2 张，可多选留 1 张。
        var extraDraws = Math.Max(0, selectables.Count - BaseDraw);
        var keepCount = Math.Min(selectables.Count, BaseKeep + extraDraws / 2);
        if (keepCount >= selectables.Count)
            return;

        Flash();
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, keepCount, keepCount);
        var keep = (await CardSelectCmd.FromCombatPile(
            choiceContext,
            hand,
            Owner,
            prefs,
            c => !IsInnateCard(c))).ToHashSet();

        foreach (var card in selectables.Where(c => !keep.Contains(c)).ToList())
            await CardPileCmd.Add(card, PileType.Discard);
    }

    private static bool IsInnateCard(CardModel card) =>
        card.Keywords.Contains(CardKeyword.Innate);
}
