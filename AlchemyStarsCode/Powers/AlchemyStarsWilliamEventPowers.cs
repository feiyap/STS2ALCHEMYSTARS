using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 蜃影：每层使青瞳造成的伤害提高 20%。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsMiragePower : AlchemyStarsPowerBase
{
    public const decimal BonusPerStack = 0.20m;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (dealer != Owner || Amount <= 0)
            return 1m;

        if (cardSource is not Cards.AlchemyStarsQingTong)
            return 1m;

        if (!props.IsPoweredAttack())
            return 1m;

        return 1m + Amount * BonusPerStack;
    }
}

/// <summary>
/// 机工印记：被弗劳尔再次命中时额外同额伤害；命中其他目标时转移。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsMechMarkPower : AlchemyStarsPowerBase
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Single;
}

/// <summary>
/// 承受余波：回合结束时结算剩余半额承受伤害（可被格挡）。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsEndureBacklashPower : AlchemyStarsPowerBase
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner) || Amount <= 0)
            return;

        Flash();
        var damage = Amount;
        await PowerCmd.Remove(this);
        await CreatureCmd.Damage(
            choiceContext,
            Owner,
            damage,
            ValueProp.Move,
            null,
            null);
    }
}
