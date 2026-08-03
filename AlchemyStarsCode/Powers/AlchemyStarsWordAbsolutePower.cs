using System.Collections.Generic;
using System.Threading.Tasks;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 言绝：每获�?3 个森属性强化格，获�?1 层飞行�?/// </summary>
[RegisterPower]
public sealed class AlchemyStarsWordAbsolutePower : ModPowerTemplate
{
    private const int EnhancedCellsPerFlying = 3;

    private int _pendingEnhancedCells;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    internal void NotifyEnhancedCellsGained(int count)
    {
        if (count <= 0)
            return;

        _pendingEnhancedCells += count;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await TryGrantFlyingFromPendingEnhancedCells(choiceContext);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner))
            return;

        await TryGrantFlyingFromPendingEnhancedCells(choiceContext);
    }

    private async Task TryGrantFlyingFromPendingEnhancedCells(PlayerChoiceContext choiceContext)
    {
        while (_pendingEnhancedCells >= EnhancedCellsPerFlying)
        {
            _pendingEnhancedCells -= EnhancedCellsPerFlying;
            await PowerCmd.Apply<AlchemyStarsFlyingPower>(
                choiceContext,
                Owner,
                1m,
                Owner,
                null);
        }
    }
}
