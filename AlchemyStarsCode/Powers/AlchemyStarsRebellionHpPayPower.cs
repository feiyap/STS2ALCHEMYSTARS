using System.Collections.Generic;
using System.Threading.Tasks;
using AlchemyStars.Cards;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Players;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// ????????????????????????????????????/// </summary>
[RegisterPower]
public sealed class AlchemyStarsRebellionHpPayPower : ModPowerTemplate
{
    private int _activeFromTurn;
    private CardModel? _pendingHpPayCard;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public void ActivateFromNextTurn(Player player)
    {
        var turn = player.PlayerCombatState?.TurnNumber ?? 0;
        _activeFromTurn = turn + 1;
    }

    private bool IsActive(Player? player)
    {
        if (player?.PlayerCombatState == null || _activeFromTurn <= 0)
            return false;

        return player.PlayerCombatState.TurnNumber >= _activeFromTurn;
    }

    public override bool TryModifyEnergyCostInCombatLate(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        var player = Owner.Player;
        if (!IsActive(player) || originalCost <= 0m)
            return false;

        if (!AlchemyStarsRebellionBurningHelper.HasRebellionBurning(card))
            return false;

        if (card is AlchemyStarsGeneratedRebellionBurningReinhardt)
            return false;

        if (player!.PlayerCombatState!.Energy >= originalCost)
            return false;

        if (Owner.CurrentHp <= originalCost)
            return false;

        _pendingHpPayCard = card;
        modifiedCost = 0m;
        return true;
    }

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (_pendingHpPayCard == null || !ReferenceEquals(cardPlay.Card, _pendingHpPayCard))
            return;

        _pendingHpPayCard = null;
        var cost = System.Math.Max(0, cardPlay.Card.EnergyCost.GetWithModifiers(CostModifiers.All));
        if (cost <= 0m)
            return;

        Flash();
        await CreatureCmd.Damage(
            new BlockingPlayerChoiceContext(),
            Owner,
            cost,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
            cardPlay.Card,
            cardPlay);
    }
}
