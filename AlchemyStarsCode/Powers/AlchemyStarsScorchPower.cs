using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 灼烧：回合开始时，每层失�?1% 最大生命值�?
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsScorchPower : AlchemyStarsPowerBase
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!participants.Contains(Owner))
            return;

        if (Amount <= 0)
            return;

        AlchemyStars.Relics.Ancients.AlchemyStarsNudgeTheFlow.CapScorchStacks(this);

        var damage = (decimal)System.Math.Ceiling((double)(Owner.MaxHp * 0.01m * Amount));
        damage = AlchemyStars.Relics.Ancients.AlchemyStarsNudgeTheFlow.ModifyScorchDamage(Owner, damage);
        if (damage <= 0)
            return;

        await CreatureCmd.Damage(
            new ThrowingPlayerChoiceContext(),
            Owner,
            damage,
            ValueProp.Unblockable | ValueProp.Unpowered,
            null,
            null);
    }
}
