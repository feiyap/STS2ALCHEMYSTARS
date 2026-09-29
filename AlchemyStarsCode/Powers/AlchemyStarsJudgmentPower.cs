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
/// 审判：受到雷属性伤害时 +1 层；到达 25 层时造成当前生命 33% 伤害并移除；回合结束 -1 层。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsJudgmentPower : AlchemyStarsPowerBase
{
    private const int TriggerThreshold = 25;
    private const decimal CurrentHpDamagePercent = 0.33m;

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
        await TryTriggerThreshold(choiceContext, Owner);
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

    public static async Task TryTriggerThreshold(
        PlayerChoiceContext choiceContext,
        Creature target,
        int threshold = TriggerThreshold)
    {
        var judgment = target.GetPower<AlchemyStarsJudgmentPower>();
        if (judgment == null || judgment.Amount < threshold || target.IsDead)
            return;

        FlashIfPossible(judgment);
        var loss = target.CurrentHp * CurrentHpDamagePercent;
        if (loss > 0m)
        {
            await CreatureCmd.Damage(
                choiceContext,
                target,
                loss,
                ValueProp.Unblockable | ValueProp.Unpowered,
                null,
                null);
        }

        await PowerCmd.Remove(judgment);
    }

    /// <summary>兼容旧调用名。</summary>
    public static Task TryTriggerStunThreshold(
        PlayerChoiceContext choiceContext,
        Creature target,
        int threshold = TriggerThreshold) =>
        TryTriggerThreshold(choiceContext, target, threshold);

    private static void FlashIfPossible(AlchemyStarsJudgmentPower judgment)
    {
        try
        {
            judgment.Flash();
        }
        catch
        {
            // 忽略闪光失败
        }
    }
}
