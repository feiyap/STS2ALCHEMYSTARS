using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Powers;

/// <summary>
/// 戒卫守望的代价：下回合开始时死亡。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsGuardDoomPower : AlchemyStarsPowerBase
{
    private int _killOnTurn;

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        AssertMutable();
        var turn = Owner.Player?.PlayerCombatState?.TurnNumber ?? 0;
        _killOnTurn = turn + 1;
        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner || Owner.IsDead)
            return;

        var turn = player.PlayerCombatState?.TurnNumber ?? 0;
        if (turn < _killOnTurn)
            return;

        Flash();
        await CreatureCmd.Kill(Owner, force: true);
        if (Owner.GetPower<AlchemyStarsGuardDoomPower>() != null)
            await PowerCmd.Remove(this);
    }
}
