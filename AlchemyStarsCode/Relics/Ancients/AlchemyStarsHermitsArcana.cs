using AlchemyStars.Cards;
using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 隐士奥秘：每回合首张攻击与首张能力费用为 0；拾起时加入诅咒「苦恼」。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsHermitsArcana : AlchemyStarsAncientRelicBase
{
    private bool _attackFreedThisTurn;
    private bool _powerFreedThisTurn;

    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        if (Owner == null)
            return;

        await CardPileCmd.AddCurseToDeck<AlchemyStarsAngst>(Owner);
        Flash();
    }

    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (Owner != null && participants.Contains(Owner.Creature))
        {
            _attackFreedThisTurn = false;
            _powerFreedThisTurn = false;
        }

        return Task.CompletedTask;
    }

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (Owner == null || card.Owner != Owner)
            return false;

        if (!CombatManager.Instance.IsInProgress)
            return false;

        var pile = card.Pile?.Type;
        if (pile is not (PileType.Hand or PileType.Play))
            return false;

        if (card.Type == CardType.Attack && !_attackFreedThisTurn)
        {
            modifiedCost = 0m;
            return true;
        }

        if (card.Type == CardType.Power && !_powerFreedThisTurn)
        {
            modifiedCost = 0m;
            return true;
        }

        return false;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || cardPlay.Card.Owner != Owner)
            return Task.CompletedTask;

        if (cardPlay.Card.Type == CardType.Attack)
            _attackFreedThisTurn = true;
        else if (cardPlay.Card.Type == CardType.Power)
            _powerFreedThisTurn = true;

        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom _)
    {
        _attackFreedThisTurn = false;
        _powerFreedThisTurn = false;
        return Task.CompletedTask;
    }
}
