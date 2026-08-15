using System.Collections.Generic;
using System.Threading.Tasks;
using AlchemyStars.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Players;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 反叛灼燃之日升级：从下一回合开始，反叛灼燃牌在能量不足时可用同等生命支付。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsRebellionHpPayPower : ModPowerTemplate
{
    private int _activeFromTurn;
    private CardModel? _pendingHpPayCard;
    private decimal _pendingHpPayCost;

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
        _pendingHpPayCost = originalCost;
        modifiedCost = 0m;
        return true;
    }

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (_pendingHpPayCard == null || !ReferenceEquals(cardPlay.Card, _pendingHpPayCard))
            return;

        var cost = _pendingHpPayCost;
        _pendingHpPayCard = null;
        _pendingHpPayCost = 0m;
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
