using System.Linq;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using AlchemyStars.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Keywords;

namespace AlchemyStars.Cards;

/// <summary>
/// ????????????
/// </summary>
internal static class AlchemyStarsCardHelpers
{
    public static bool HasOtherTagInHand(CardModel self, Player owner, CardTag tag)
    {
        var hand = owner.PlayerCombatState?.Hand.Cards;
        if (hand == null)
            return false;

        return hand.Any(card => !ReferenceEquals(card, self) && card.Tags.Contains(tag));
    }

    public static bool IsFirstCardPlayedThisTurn(CardModel self, Player owner, ICombatState? combatState)
    {
        if (combatState == null)
            return true;

        var plays = CombatManager.Instance.History.CardPlaysStarted
            .Where(entry => entry.HappenedThisTurn(combatState) && entry.CardPlay.Card.Owner == owner)
            .ToList();

        return plays.Count == 0 ||
               (plays.Count == 1 && ReferenceEquals(plays[0].CardPlay.Card, self));
    }

    public static CardModel? FindOverloadInHand(Player owner) =>
        owner.PlayerCombatState?.Hand.Cards.FirstOrDefault(card => card is AlchemyStarsGeneratedOverload);

    public static async Task<bool> TryConsumeOverloadFromHand(
        PlayerChoiceContext choiceContext,
        Player owner)
    {
        var overload = FindOverloadInHand(owner);
        if (overload == null)
            return false;

        await CardCmd.Exhaust(choiceContext, overload);
        return true;
    }

    public static bool HasThunderKeyword(CardModel card) =>
        card.Keywords.Contains(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder));

    public static bool HasForestKeyword(CardModel card) =>
        card.Keywords.Contains(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest));

    public static bool HasWaterKeyword(CardModel card) =>
        card.Keywords.Contains(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water));

    public static bool HasFireKeyword(CardModel card) =>
        card.Keywords.Contains(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire));

    public static bool IsAttributeCard(CardModel card) =>
        HasForestKeyword(card) || HasThunderKeyword(card) || HasWaterKeyword(card) || HasFireKeyword(card);

    public static bool IsTeaPartyMember(CardModel card) =>
        card.Tags.Contains(AlchemyStarsCardTags.ShadowTownTeaParty);

    public static async Task TryTriggerTeaPartyOnPlay(
        PlayerChoiceContext choiceContext,
        CardModel card,
        Player owner)
    {
        if (!IsTeaPartyMember(card))
            return;

        // 冷却中，或场上仍有未消耗的折扣时，不再叠加。
        if (AlchemyStarsForestState.GetTeaPartyCooldown(owner) > 0)
            return;

        if (owner.Creature.GetPowerAmount<AlchemyStarsTeaPartyDiscountPower>() > 0)
            return;

        await PowerCmd.Apply<AlchemyStarsTeaPartyDiscountPower>(
            choiceContext,
            owner.Creature,
            1m,
            owner.Creature,
            card);

        // 冷却在折扣被消耗时启动（见 TeaPartyDiscountPower.BeforeCardPlayed）。
    }

    public static async Task TryDrawLegionCommanderFromDrawPile(
        PlayerChoiceContext choiceContext,
        Player owner,
        CardModel source)
    {
        var drawPile = PileType.Draw.GetPile(owner);
        var legionCards = drawPile.Cards
            .Where(card => card.Tags.Contains(AlchemyStarsCardTags.LegionCommander))
            .ToList();

        if (legionCards.Count == 0)
            return;

        var picked = owner.RunState.Rng.CombatTargets.NextItem(legionCards);
        if (picked != null)
            await CardPileCmd.Add(picked, PileType.Hand);
    }

    public static async Task TryExecuteBelowHpThreshold(
        PlayerChoiceContext choiceContext,
        Creature target,
        decimal thresholdPercent = 0.1m)
    {
        if (target.IsDead || target.MaxHp <= 0)
            return;

        if (target.CurrentHp / target.MaxHp > thresholdPercent)
            return;

        await CreatureCmd.Damage(
            choiceContext,
            target,
            target.CurrentHp,
            ValueProp.Unblockable | ValueProp.Unpowered,
            null,
            null);
    }

    public static async Task ClearEnemyDefenses(
        PlayerChoiceContext choiceContext,
        Creature target)
    {
        if (target.Block > 0)
            await CreatureCmd.LoseBlock(choiceContext, target, target.Block, null);

        if (target.HasPower<SlipperyPower>())
            await PowerCmd.Remove<SlipperyPower>(target);

        if (target.HasPower<BufferPower>())
            await PowerCmd.Remove<BufferPower>(target);
    }

    public static async Task ClearPenetratingDefenses(
        PlayerChoiceContext choiceContext,
        Creature target)
    {
        await ClearEnemyDefenses(choiceContext, target);

        if (target.HasPower<PlatingPower>())
            await PowerCmd.Remove<PlatingPower>(target);

        if (target.HasPower<HardToKillPower>())
            await PowerCmd.Remove<HardToKillPower>(target);
    }

    public static async Task IncrementStackableBuffs(
        PlayerChoiceContext choiceContext,
        Creature creature,
        decimal amount,
        Creature applier,
        CardModel? source)
    {
        foreach (var power in creature.Powers.ToList())
        {
            if (power.Type != PowerType.Buff || power.StackType != PowerStackType.Counter || power.Amount <= 0)
                continue;

            await PowerCmd.ModifyAmount(choiceContext, power, amount, applier, source);
        }
    }

    public static async Task TryApplyRandomDebuff(
        PlayerChoiceContext choiceContext,
        Creature target,
        decimal amount,
        Creature applier,
        CardModel? source)
    {
        if (target.IsDead || amount <= 0)
            return;

        var rng = applier.Player!.RunState.Rng.CombatTargets;
        var roll = rng.NextInt(3);
        switch (roll)
        {
            case 0:
                await PowerCmd.Apply<WeakPower>(choiceContext, target, amount, applier, source);
                break;
            case 1:
                await PowerCmd.Apply<VulnerablePower>(choiceContext, target, amount, applier, source);
                break;
            default:
                await PowerCmd.Apply<FrailPower>(choiceContext, target, amount, applier, source);
                break;
        }
    }

    public static Creature? FindLowestHpPercentEnemy(ICombatState? combatState)
    {
        if (combatState == null)
            return null;

        return combatState.HittableEnemies
            .Where(enemy => !enemy.IsDead && enemy.MaxHp > 0)
            .OrderBy(enemy => enemy.CurrentHp / enemy.MaxHp)
            .FirstOrDefault();
    }

    /// <summary>
    /// 是否与你同距离：一般以 HittableEnemies[0] 为最近；
    /// 帝王蟹左右钳等部位视为全体同距离。
    /// </summary>
    public static bool AreEnemiesAtSameDistance(IReadOnlyList<Creature> enemies)
    {
        if (enemies.Count <= 1)
            return true;

        var encounter = enemies[0].CombatState?.Encounter;
        if (encounter is KaiserCrabBoss)
            return true;

        // 帝王蟹式部位：全部为左右钳（Crusher / Rocket）
        return enemies.All(enemy =>
            enemy.Monster is Crusher or Rocket);
    }

    /// <summary>
    /// 目标是否为最近敌人（同距离时所有目标均视为最近）。
    /// </summary>
    public static bool IsNearestEnemy(Creature target, IReadOnlyList<Creature> enemies)
    {
        if (enemies.Count == 0)
            return false;

        if (AreEnemiesAtSameDistance(enemies))
            return enemies.Any(enemy => ReferenceEquals(enemy, target));

        return ReferenceEquals(target, enemies[0]);
    }

    public static async Task DoubleStackableDebuffs(
        PlayerChoiceContext choiceContext,
        Creature creature,
        Creature applier,
        CardModel? source)
    {
        foreach (var power in creature.Powers.ToList())
        {
            if (power.Type != PowerType.Debuff || power.StackType != PowerStackType.Counter || power.Amount <= 0)
                continue;

            await PowerCmd.ModifyAmount(choiceContext, power, power.Amount, applier, source);
        }
    }
}

