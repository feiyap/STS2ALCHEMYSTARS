using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using AlchemyStars.Keywords;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 极光时刻：每消耗 15 点光能，当前手牌本回合耗能变为 0，并临时获得虚无。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsAuroraMomentPower : ModPowerTemplate
{
    private const int LightEnergyPerTrigger = 15;

    private int _pendingConsumed;
    private readonly HashSet<CardModel> _temporaryEtherealCards = [];

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<string> RegisteredKeywordIds => [AlchemyStarsKeywordIds.AuroraMoment];

    internal void NotifyLightEnergyConsumed(int count)
    {
        if (count <= 0)
            return;

        var player = Owner.Player;
        if (player == null)
            return;

        _pendingConsumed += count;
        while (_pendingConsumed >= LightEnergyPerTrigger)
        {
            _pendingConsumed -= LightEnergyPerTrigger;
            Flash();
            ApplyHandDiscount(player);
        }
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!_temporaryEtherealCards.Remove(cardPlay.Card))
            return;

        if (cardPlay.Card.Keywords.Contains(CardKeyword.Ethereal))
            CardCmd.RemoveKeyword(cardPlay.Card, CardKeyword.Ethereal);

        await Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
            return;

        ClearTemporaryEthereal();
        await Task.CompletedTask;
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        ClearTemporaryEthereal();
        await Task.CompletedTask;
    }

    private void ApplyHandDiscount(Player player)
    {
        var hand = player.PlayerCombatState?.Hand.Cards;
        if (hand == null)
            return;

        foreach (var card in hand.ToList())
        {
            card.EnergyCost.SetThisTurn(0);

            if (card.Keywords.Contains(CardKeyword.Ethereal))
                continue;

            CardCmd.ApplyKeyword(card, CardKeyword.Ethereal);
            _temporaryEtherealCards.Add(card);
        }
    }

    private void ClearTemporaryEthereal()
    {
        foreach (var card in _temporaryEtherealCards.ToList())
        {
            if (card.Keywords.Contains(CardKeyword.Ethereal))
                CardCmd.RemoveKeyword(card, CardKeyword.Ethereal);
        }

        _temporaryEtherealCards.Clear();
    }
}
