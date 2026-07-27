using System.Collections.Generic;
using System.Threading.Tasks;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 万应灵药：对目标施加易伤；未被格挡的伤害转为血量，受 Amount 上限约束。
/// Amount = 剩余可转化生命上限。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsPanaceaPower : ModPowerTemplate
{
    public Creature? MarkedTarget { get; set; }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<VulnerablePower>(2m)
    ];

    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        if (dealer != Owner || Amount <= 0 || MarkedTarget == null)
            return;

        if (!ReferenceEquals(target, MarkedTarget))
            return;

        var heal = System.Math.Min(Amount, result.UnblockedDamage);
        if (heal <= 0)
            return;

        Flash();
        await CreatureCmd.Heal(Owner, heal);
        await PowerCmd.ModifyAmount(choiceContext, this, -heal, Owner, cardSource);
        if (Amount <= 0)
            await PowerCmd.Remove(this);
    }
}
