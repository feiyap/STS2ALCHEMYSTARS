using System.Collections.Generic;
using System.Threading.Tasks;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 审判：受到雷属性伤害时叠加 1 层颤栗；回合结束时失�?1 层�?/// </summary>
[RegisterPower]
public sealed class AlchemyStarsJudgmentPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<string> RegisteredKeywordIds => ["judgment"];

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner || result.TotalDamage <= 0)
            return;

        if (LightMechanicDamageContext.CurrentElement != LightElement.Thunder &&
            LightMechanicDamageContext.CurrentElement != LightElement.Prismatic)
            return;

        await PowerCmd.Apply<AlchemyStarsTremorPower>(
            choiceContext,
            Owner,
            1m,
            dealer,
            cardSource);

        await AlchemyStarsTremorPower.TryTriggerStunThreshold(choiceContext, Owner);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
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
