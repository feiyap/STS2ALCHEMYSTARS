using System.Linq;
using AlchemyStars.Characters;
using AlchemyStars.Mechanics;
using AlchemyStars.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 与世隔绝者的评估：开战赋予敌人森/火/雷/水随机属性，并按克制修正伤害。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsIsolatorsAppraisal : AlchemyStarsAncientRelicBase
{
    public override async Task BeforeCombatStart()
    {
        if (Owner?.Creature.CombatState == null)
            return;

        Flash();
        foreach (var enemy in EnumerateEnemies(Owner.Creature.CombatState).ToList())
            await AssignRandomAttribute(enemy);
    }

    public override async Task AfterCreatureAddedToCombat(Creature creature)
    {
        if (Owner == null || !IsEnemy(creature))
            return;

        if (creature.GetPower<AlchemyStarsEnemyAttributePower>() != null)
            return;

        Flash();
        await AssignRandomAttribute(creature);
    }

    private async Task AssignRandomAttribute(Creature enemy)
    {
        if (Owner == null || enemy.IsDead)
            return;

        var element = Owner.RunState.Rng.CombatTargets.NextItem(LightElementExtensions.BaseElements);
        await PowerCmd.Apply<AlchemyStarsEnemyAttributePower>(
            new ThrowingPlayerChoiceContext(),
            enemy,
            AlchemyStarsEnemyAttributePower.Encode(element),
            Owner.Creature,
            null);
    }

    private static IEnumerable<Creature> EnumerateEnemies(ICombatState combatState) =>
        combatState.GetCreaturesOnSide(CombatSide.Enemy)
            .Concat(combatState.HittableEnemies)
            .Where(c => !c.IsDead)
            .Distinct();

    private static bool IsEnemy(Creature creature) =>
        creature.Side == CombatSide.Enemy;

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (Owner == null || dealer?.Player != Owner || target == null)
            return 1m;

        var attackElement = LightMechanicDamageContext.CurrentElement;
        if (attackElement == null)
            return 1m;

        if (attackElement == LightElement.Prismatic)
            return 1.2m;

        var power = target.GetPower<AlchemyStarsEnemyAttributePower>();
        if (power == null)
            return 1m;

        var defense = power.Element;
        if (Beats(attackElement.Value, defense))
            return 1.4m;
        if (Beats(defense, attackElement.Value))
            return 0.8m;

        return 1m;
    }

    private static bool Beats(LightElement attacker, LightElement defender) =>
        (attacker, defender) switch
        {
            (LightElement.Thunder, LightElement.Water) => true,
            (LightElement.Water, LightElement.Fire) => true,
            (LightElement.Fire, LightElement.Forest) => true,
            (LightElement.Forest, LightElement.Thunder) => true,
            _ => false,
        };
}
