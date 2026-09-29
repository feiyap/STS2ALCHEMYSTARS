using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 极光时刻：每打出 15 张牌，当前手牌本回合耗能变为 0。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsAuroraMomentPower : AlchemyStarsPowerBase
{
    private const int CardsPerTrigger = 15;

    private int _pendingPlayed;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != Owner)
            return;

        _pendingPlayed++;
        while (_pendingPlayed >= CardsPerTrigger)
        {
            _pendingPlayed -= CardsPerTrigger;
            Flash();
            ApplyHandDiscount(Owner.Player);
        }

        await Task.CompletedTask;
    }

    private static void ApplyHandDiscount(Player? player)
    {
        var hand = player?.PlayerCombatState?.Hand.Cards;
        if (hand == null)
            return;

        foreach (var card in hand.ToList())
            card.EnergyCost.SetThisTurn(0);
    }
}
