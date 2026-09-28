using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 甜蜜麦克风：战斗开始自动从左到右打出手牌，并眩晕所有敌人。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsSweetMicrophone : AlchemyStarsAncientRelicBase
{
    public override async Task AfterAutoPrePlayPhaseEnteredLate(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner || Owner == null)
            return;

        if (Owner.PlayerCombatState.TurnNumber != 1)
            return;

        Flash();

        var combatState = Owner.Creature.CombatState;
        if (combatState == null)
            return;

        // 从左到右自动打出手牌。
        var hand = PileType.Hand.GetPile(Owner).Cards.ToList();
        foreach (var card in hand)
        {
            if (CombatManager.Instance.IsOverOrEnding)
                break;

            if (!card.CanPlay())
                continue;

            var target = ResolveTarget(card, combatState);
            await card.SpendResources();
            await CardCmd.AutoPlay(choiceContext, card, target, AutoPlayType.Default, skipXCapture: true);
        }

        // 眩晕所有敌人（玩家眩晕 API 不合适则省略）。
        foreach (var enemy in combatState.HittableEnemies.ToList())
        {
            if (enemy.IsDead)
                continue;

            await CreatureCmd.Stun(enemy);
        }
    }

    private Creature? ResolveTarget(CardModel card, ICombatState combatState)
    {
        Rng combatTargets = Owner!.RunState.Rng.CombatTargets;
        return card.TargetType switch
        {
            TargetType.AnyEnemy => combatState.HittableEnemies.FirstOrDefault(),
            TargetType.AnyAlly => combatTargets.NextItem(
                combatState.Allies.Where(c => c is { IsAlive: true, IsPlayer: true } && c != Owner.Creature)),
            TargetType.AnyPlayer => Owner.Creature,
            _ => null,
        };
    }
}
