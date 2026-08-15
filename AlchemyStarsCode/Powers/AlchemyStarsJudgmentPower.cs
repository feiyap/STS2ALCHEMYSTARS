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
/// 审判：受到雷属性伤害时自身 +1 层；到达 25 层时眩晕并移除所有审判；回合结束时 -1 层。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsJudgmentPower : ModPowerTemplate
{
    private const int StunThreshold = 25;

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

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

        await PowerCmd.ModifyAmount(choiceContext, this, 1m, dealer, cardSource);
        await TryTriggerStunThreshold(choiceContext, Owner);
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

    public static async Task TryTriggerStunThreshold(
        PlayerChoiceContext choiceContext,
        Creature target,
        int threshold = StunThreshold)
    {
        var judgment = target.GetPower<AlchemyStarsJudgmentPower>();
        if (judgment == null || judgment.Amount < threshold || target.IsDead)
            return;

        await CreatureCmd.Stun(target);
        await PowerCmd.Remove(judgment);
    }
}
