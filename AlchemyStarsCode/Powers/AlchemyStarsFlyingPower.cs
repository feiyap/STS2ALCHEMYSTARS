using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 飞行：受到的攻击伤害减半；回合开始时失去 1 层�?/// </summary>
[RegisterPower]
public sealed class AlchemyStarsFlyingPower : ModPowerTemplate
{
    private const string DamageDecreaseKey = "DamageDecrease";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<string> RegisteredKeywordIds => ["flying"];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(DamageDecreaseKey, 50m)
    ];

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target != Owner || !props.IsPoweredAttack())
            return 1m;

        return DynamicVars[DamageDecreaseKey].BaseValue / 100m;
    }

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!participants.Contains(Owner) || Owner.IsDead || Amount <= 0)
            return;

        var ownerSide = Owner.IsPlayer ? CombatSide.Player : CombatSide.Enemy;
        if (side != ownerSide)
            return;

        Flash();
        await PowerCmd.Decrement(this);
    }
}
