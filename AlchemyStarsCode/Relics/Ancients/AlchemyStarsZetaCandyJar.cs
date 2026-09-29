using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 泽塔糖果罐：前 2 回合自动喝下 1 瓶可对自己使用的随机药水。
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

        if (Owner.PlayerCombatState?.TurnNumber > AutoDrinkTurns)
            return;

        Flash();
        await DrinkRandomSelfPotion(new ThrowingPlayerChoiceContext());
    }

    public override Task AfterCombatEnd(CombatRoom _) => Task.CompletedTask;

    /// <summary>
    /// 临时入栏后立即对自己喝下；无法合法使用则丢弃。
    /// </summary>
    private async Task DrinkRandomSelfPotion(PlayerChoiceContext choiceContext)
    {
        var canonical = PickRandomSelfDrinkablePotion(Owner!.RunState.Rng.CombatPotionGeneration);
        if (canonical == null)
            return;

        var potion = canonical.ToMutable();
        var procure = await PotionCmd.TryToProcure(potion, Owner);
        if (!procure.success)
            return;

        if (!potion.IsValidTarget(Owner.Creature))
        {
            await PotionCmd.Discard(potion);
            return;
        }

        await potion.OnUseWrapper(choiceContext, Owner.Creature);
    }

    private PotionModel? PickRandomSelfDrinkablePotion(Rng rng)
    {
        var options = PotionFactory.GetPotionOptions(Owner!)
            .Where(IsSelfDrinkableInCombat)
            .ToList();
        if (options.Count == 0)
            return null;

        var roll = rng.NextFloat();
        var rarity = roll <= 0.1f
            ? PotionRarity.Rare
            : roll <= 0.35f
                ? PotionRarity.Uncommon
                : PotionRarity.Common;

        var rarityPool = options.Where(p => p.Rarity == rarity).ToList();
        if (rarityPool.Count == 0)
            rarityPool = options;

        return rng.NextItem(rarityPool);
    }

    private static bool IsSelfDrinkableInCombat(PotionModel potion)
    {
        if (!potion.CanBeGeneratedInCombat)
            return false;

        if (potion.Usage is PotionUsage.Automatic or PotionUsage.None)
            return false;

        return potion.TargetType is TargetType.Self or TargetType.AnyPlayer;
    }
}
