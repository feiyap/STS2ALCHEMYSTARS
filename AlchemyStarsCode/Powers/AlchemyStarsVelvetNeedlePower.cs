using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
/// 南极光绒针：回合结束时爆裂，每层造成 1 点水属性伤害并恢复持有�?1 点生命�?/// </summary>
[RegisterPower]
public sealed class AlchemyStarsVelvetNeedlePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<string> RegisteredKeywordIds => ["velvet_needle"];

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Enemy || Owner.IsDead || Amount <= 0)
            return;

        var applier = participants.FirstOrDefault(creature => creature.IsPlayer && creature.IsAlive);
        if (applier?.Player == null)
            return;

        var stacks = (int)Amount;
        await PowerCmd.Remove(this);

        for (var i = 0; i < stacks; i++)
        {
            if (Owner.IsDead)
                break;

            await LightMechanic.DealElementalAttackDamage(
                choiceContext,
                applier.Player,
                null,
                Owner,
                1m,
                LightElement.Water);

            if (!applier.IsDead)
                await CreatureCmd.Heal(applier, 1);
        }
    }
}
