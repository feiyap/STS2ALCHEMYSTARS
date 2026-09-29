using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 隐士奥秘：每回合第 1 张攻击或能力牌费用为 0；拾起时加入诅咒「苦恼」。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsHermitsArcana : AlchemyStarsAncientRelicBase
{
    private bool _freedThisTurn;

    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        if (Owner == null)
            return;

        await CardPileCmd.AddCurseToDeck<Writhe>(Owner);
        Flash();
    }

    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (Owner != null && participants.Contains(Owner.Creature))
            _freedThisTurn = false;

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

        if (_freedThisTurn)
            return false;

        if (card.Type is not (CardType.Attack or CardType.Power))
            return false;

        modifiedCost = 0m;
        return true;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || cardPlay.Card.Owner != Owner)
            return Task.CompletedTask;

        if (cardPlay.Card.Type is CardType.Attack or CardType.Power)
            _freedThisTurn = true;

        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom _)
    {
        _freedThisTurn = false;
        return Task.CompletedTask;
    }
}
