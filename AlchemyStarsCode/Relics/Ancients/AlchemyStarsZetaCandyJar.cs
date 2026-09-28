using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 泽塔糖果罐：前 2 回合自动喝一瓶随机药水。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsZetaCandyJar : AlchemyStarsAncientRelicBase
{
    private const int AutoDrinkTurns = 2;

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (Owner == null || !participants.Contains(Owner.Creature))
            return;

        if (Owner.PlayerCombatState.TurnNumber > AutoDrinkTurns)
            return;

        Flash();
        await DrinkRandomPotion(new ThrowingPlayerChoiceContext(), combatState);
    }

    public override Task AfterCombatEnd(CombatRoom _) => Task.CompletedTask;

    /// <summary>
    /// 尝试获取并立即使用一瓶随机战斗药水。
    /// </summary>
    private async Task DrinkRandomPotion(PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        var potion = PotionFactory
            .CreateRandomPotionInCombat(Owner!, Owner!.RunState.Rng.CombatPotionGeneration)
            .ToMutable();

        var procure = await PotionCmd.TryToProcure(potion, Owner);
        if (!procure.success)
            return;

        Creature? target = null;
        if (potion.TargetType.IsSingleTarget())
        {
            target = combatState.HittableEnemies.FirstOrDefault()
                     ?? Owner.Creature;
        }
        else if (potion.TargetType != TargetType.TargetedNoCreature)
        {
            target = Owner.Creature;
        }

        if (!potion.IsValidTarget(target))
            return;

        await potion.OnUseWrapper(choiceContext, target);
    }
}
