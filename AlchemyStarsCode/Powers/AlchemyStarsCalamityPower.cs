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
/// 灾厄：敌人回合结束时，每层造成 1 点固伤。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsCalamityPower : AlchemyStarsPowerBase
{
    private const decimal DamagePerStack = 1m;

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Enemy || Owner.IsDead || Amount <= 0)
            return;

        var damage = DamagePerStack * Amount;
        if (damage <= 0m)
            return;

        Flash();
        await CreatureCmd.Damage(
            choiceContext,
            Owner,
            damage,
            ValueProp.Unblockable | ValueProp.Unpowered,
            null,
            null);
    }
}
