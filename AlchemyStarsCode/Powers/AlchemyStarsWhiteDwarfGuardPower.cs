using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 白矮星守护：受到攻击前对攻击者施加 1 层虚弱；己方回合开始时失去 1 层。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsWhiteDwarfGuardPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeAttack(AttackCommand command)
    {
        if (Amount <= 0)
            return;

        var attacker = command.Attacker;
        if (attacker == null || attacker.IsDead || attacker == Owner)
            return;

        if (!command.DamageProps.IsPoweredAttack())
            return;

        var ownerSide = Owner.IsPlayer ? CombatSide.Player : CombatSide.Enemy;
        if (command.TargetSide != ownerSide)
            return;

        Flash();
        await PowerCmd.Apply<WeakPower>(
            new BlockingPlayerChoiceContext(),
            attacker,
            1m,
            Owner,
            command.ModelSource as CardModel);
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
