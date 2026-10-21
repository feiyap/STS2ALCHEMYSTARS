using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Monsters.Powers;

/// <summary>
/// 悲怆：力量同时加成攻击与格挡。格挡被打破时，当前力量减半。
/// 攻击加成来自原版力量；格挡加成由祭剑座结算。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsAbsoluteResolvePower : AlchemyStarsSwordAltarPowerBase
{
    protected override string PlaceholderIcon => "AlchemyStarsAbsoluteResolvePower";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override int DisplayAmount => 0;

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner || !result.WasBlockBroken)
            return;

        var strength = Owner.GetPower<StrengthPower>();
        if (strength == null || strength.Amount <= 0)
            return;

        Flash();
        var next = (int)(strength.Amount / 2m);
        if (next <= 0)
            await PowerCmd.Remove(strength);
        else
            strength.SetAmount(next);
    }
}
