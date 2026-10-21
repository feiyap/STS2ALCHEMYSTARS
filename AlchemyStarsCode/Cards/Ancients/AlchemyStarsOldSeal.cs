using System.Collections.Generic;
using AlchemyStars.Mechanics;
using AlchemyStars.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 旧印卡标记。星辰纹章用它识别需要永久降费的牌。
/// </summary>
public interface IAlchemyStarsOldSeal
{
}

/// <summary>
/// 旧印卡共用结算：威能、群体属性伤害。
/// </summary>
internal static class AlchemyStarsOldSeal
{
    public const decimal HunterMight = 10m;
    public const decimal AltarMight = 40m;

    public static int GetMight(Player? player) =>
        player == null ? 0 : (int)player.Creature.GetPowerAmount<AlchemyStarsOldSealMightPower>();

    public static async Task GainMight(
        PlayerChoiceContext choiceContext,
        Player player,
        CardModel source,
        decimal amount)
    {
        if (amount <= 0)
            return;

        await PowerCmd.Apply<AlchemyStarsOldSealMightPower>(
            choiceContext,
            player.Creature,
            amount,
            player.Creature,
            source);
    }

    /// <summary>
    /// 抽到或生成进手牌时获得寂静猎兵威能。
    /// </summary>
    public static async Task GainHunterMightIfEnteredHand(
        PlayerChoiceContext choiceContext,
        CardModel self,
        CardModel card)
    {
        if (!ReferenceEquals(self, card) || self.Pile?.Type != PileType.Hand || self.Owner == null)
            return;

        await GainMight(choiceContext, self.Owner, self, HunterMight);
    }

    public static async Task<int> ConsumeAllMight(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        var might = player.Creature.GetPower<AlchemyStarsOldSealMightPower>();
        if (might == null || might.Amount <= 0)
            return 0;

        var amount = (int)might.Amount;
        await PowerCmd.Remove(might);
        return amount;
    }

    /// <summary>
    /// 戒卫座、焚霜座：先把力量等加算算进 {Damage}，再乘威能并加上固定值，最后吃易伤、虚弱、暴击。
    /// 属性格增伤留到实际攻击里乘一次，避免预览和结算各乘一遍。
    /// </summary>
    public static decimal ScaleDamage(CardModel card, Creature? target, decimal flat, CardPlay? cardPlay)
    {
        var owner = card.Owner;
        if (owner == null)
            return 0m;

        var might = GetMight(owner);
        var baseDamage = card.DynamicVars.Damage.BaseValue;
        var combat = card.CombatState ?? owner.Creature.CombatState;
        var run = owner.RunState;
        if (combat == null || run == null)
            return Math.Max(0m, baseDamage * might + flat);

        var preview = cardPlay == null ? CardPreviewMode.MultiCreatureTargeting : CardPreviewMode.None;
        using var suppressOutgoing = LightMechanicDamageContext.SuppressOutgoing();
        var preMult = Hook.ModifyDamage(
            run,
            combat,
            target,
            owner.Creature,
            baseDamage,
            ValueProp.Move,
            card,
            cardPlay,
            ModifyDamageHookType.Additive,
            preview,
            out _);

        var enchantment = card.Enchantment;
        if (enchantment != null)
            preMult *= enchantment.EnchantDamageMultiplicative(preMult, ValueProp.Move);

        var scaled = preMult * might + flat;
        if (scaled <= 0m)
            return 0m;

        return Hook.ModifyDamage(
            run,
            combat,
            target,
            owner.Creature,
            scaled,
            ValueProp.Move,
            null,
            cardPlay,
            ModifyDamageHookType.Multiplicative | ModifyDamageHookType.Cap,
            preview,
            out _);
    }

    public static async Task DoubleMight(
        PlayerChoiceContext choiceContext,
        Player player,
        CardModel source)
    {
        var current = GetMight(player);
        if (current <= 0)
            return;

        await GainMight(choiceContext, player, source, current);
    }

    public static async Task DealToAll(
        PlayerChoiceContext choiceContext,
        Player player,
        CardModel card,
        CardPlay cardPlay,
        decimal damage,
        Mechanics.LightElement element,
        int hits)
    {
        if (hits <= 0 || damage <= 0 || player.Creature.CombatState == null)
            return;

        var animated = false;
        for (var hit = 0; hit < hits; hit++)
        {
            foreach (var enemy in player.Creature.CombatState.HittableEnemies.ToList())
            {
                if (enemy.IsDead)
                    continue;

                await Mechanics.LightMechanic.DealElementalAttackDamage(
                    choiceContext,
                    player,
                    card,
                    enemy,
                    damage,
                    element,
                    cardPlay,
                    playAttackerAnim: !animated);
                animated = true;
            }
        }
    }
}

/// <summary>
/// 戒卫座、焚霜座共用：按威能放大已经计入力量的伤害，再造成属性攻击。
/// </summary>
public abstract class AlchemyStarsOldSealAttackCard : ModCardTemplate, IAlchemyStarsOldSeal
{
    protected AlchemyStarsOldSealAttackCard(
        int energyCost,
        CardType type,
        CardRarity rarity,
        TargetType target,
        bool showInCardLibrary)
        : base(energyCost, type, rarity, target, showInCardLibrary)
    {
    }

    protected async Task DealScaledToAll(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        decimal flat,
        LightElement element)
    {
        if (Owner.Creature.CombatState == null)
            return;

        var hits = new List<(Creature Enemy, decimal Damage)>();
        foreach (var enemy in Owner.Creature.CombatState.HittableEnemies.ToList())
        {
            if (enemy.IsDead)
                continue;

            var damage = AlchemyStarsOldSeal.ScaleDamage(this, enemy, flat, cardPlay);
            if (damage > 0m)
                hits.Add((enemy, damage));
        }

        if (hits.Count == 0)
            return;

        await WithoutEnchantment(async () =>
        {
            var animated = false;
            foreach (var (enemy, damage) in hits)
            {
                if (enemy.IsDead)
                    continue;

                using var resolvedAttack = LightMechanicDamageContext.BeginResolvedAttack();
                await LightMechanic.DealElementalAttackDamage(
                    choiceContext,
                    Owner,
                    this,
                    enemy,
                    damage,
                    element,
                    cardPlay,
                    playAttackerAnim: !animated,
                    unpowered: true);
                animated = true;
            }
        });
    }

    /// <summary>
    /// 伤害数字里已经吃过附魔。打出时先摘掉，避免无力量的攻击再乘一次。
    /// </summary>
    private async Task WithoutEnchantment(Func<Task> action)
    {
        var enchantment = Enchantment;
        if (enchantment == null)
        {
            await action();
            return;
        }

        var amount = enchantment.Amount;
        var clone = (EnchantmentModel)enchantment.MutableClone();
        ClearEnchantmentInternal();
        try
        {
            await action();
        }
        finally
        {
            EnchantInternal(clone, amount);
        }
    }
}
