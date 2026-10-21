using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using AlchemyStars.Monsters;

namespace AlchemyStars.Monsters.Powers;

/// <summary>
/// 解封：月光。受到的伤害变为 1，每次受伤减少 1 层。清空时击破。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsMoonUnsealPower : AlchemyStarsSwordAltarPowerBase
{
    protected override string PlaceholderIcon => "AlchemyStarsMoonUnsealPower";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool ShouldScaleInMultiplayer => false;

    public override decimal ModifyDamageCap(
        Creature? target,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target != Owner || Amount <= 0)
            return decimal.MaxValue;

        return 1m;
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner || Amount <= 0 || result.UnblockedDamage <= 0)
            return;

        var emptying = Amount <= 1;
        await PowerCmd.Decrement(this);
        if (emptying && Owner.Monster is AlchemyStarsSwordAltar altar)
            await altar.OnMoonUnsealBroken();
    }
}
