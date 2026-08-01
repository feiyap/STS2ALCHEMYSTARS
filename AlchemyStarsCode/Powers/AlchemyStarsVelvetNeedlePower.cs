using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 南极光绒针：敌人回合结束时一次性引爆全部层数，造成等额水属性伤害并为施加者恢复等额生命。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsVelvetNeedlePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<string> RegisteredKeywordIds => [AlchemyStarsKeywordIds.VelvetNeedle];

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        // 挂在敌人身上：在敌方回合结束时结算；施加者不在 participants 中，需用 Applier。
        if (side != CombatSide.Enemy || !participants.Contains(Owner) || Owner.IsDead || Amount <= 0)
            return;

        var applier = ResolveApplier();
        if (applier?.Player == null)
            return;

        Flash();
        var stacks = Amount;
        await PowerCmd.Remove(this);

        await LightMechanic.DealElementalAttackDamage(
            choiceContext,
            applier.Player,
            null,
            Owner,
            stacks,
            LightElement.Water);

        if (!applier.IsDead)
            await CreatureCmd.Heal(applier, stacks);
    }

    private Creature? ResolveApplier()
    {
        if (Applier is { IsAlive: true, Player: not null })
            return Applier;

        return Owner.CombatState?.PlayerCreatures
            .FirstOrDefault(creature => creature.IsAlive && creature.IsPlayer);
    }
}
