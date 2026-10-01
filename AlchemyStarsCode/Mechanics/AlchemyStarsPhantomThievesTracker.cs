using System.Collections.Generic;
using AlchemyStars.Cards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace AlchemyStars.Mechanics;

/// <summary>
/// 心之怪盗团：追踪单回合受击次数，以及破盾触发追击（由卡牌自身 AfterDamageReceived 调用）。
/// </summary>
public static class AlchemyStarsPhantomThievesTracker
{
    private const int AllOutAttackThreshold = 7;

    private static readonly Dictionary<Creature, int> HitsThisTurn = new();

    public static void ResetTurn() => HitsThisTurn.Clear();

    public static async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target.IsDead || target.Side != CombatSide.Enemy)
            return;

        if (dealer?.Player == null || dealer.Side != CombatSide.Player)
            return;

        // 总攻击：累计本回合被攻击次数。
        if (props.IsPoweredAttack())
        {
            HitsThisTurn.TryGetValue(target, out var hits);
            hits++;
            HitsThisTurn[target] = hits;
            if (hits == AllOutAttackThreshold)
            {
                await AlchemyStarsRare3.TryAutoPlay(
                    choiceContext,
                    dealer.Player,
                    target,
                    requireInHand: false);
            }
        }

        // 追击：格挡被清空。
        if (result.WasBlockBroken)
        {
            await AlchemyStarsRare3.TryAutoPlay(
                choiceContext,
                dealer.Player,
                target,
                requireInHand: true);
        }
    }

    public static async Task AfterCreatureStunned(
        PlayerChoiceContext choiceContext,
        Creature target)
    {
        if (target.IsDead || target.Side != CombatSide.Enemy || target.CombatState == null)
            return;

        foreach (var player in target.CombatState.Players)
        {
            await AlchemyStarsRare3.TryAutoPlay(
                choiceContext,
                player,
                target,
                requireInHand: true);
        }
    }
}
