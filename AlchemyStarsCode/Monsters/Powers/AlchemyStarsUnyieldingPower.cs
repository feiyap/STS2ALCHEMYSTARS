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
/// 不屈：层数为剩余可承受的攻击次数。每次受击掉 1 层，清空后眩晕。
/// 受到的未格挡伤害会在本回合变成等量临时力量，并在回合结束时等量治疗。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsUnyieldingPower : AlchemyStarsSwordAltarPowerBase
{
    protected override string PlaceholderIcon => "AlchemyStarsLockPower";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool ShouldScaleInMultiplayer => false;

    private AlchemyStarsSwordAltar? Altar => Owner.Monster as AlchemyStarsSwordAltar;

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner || dealer?.IsPlayer != true || !props.IsPoweredAttack() || Amount <= 0)
            return;

        var damage = result.UnblockedDamage;
        if (damage > 0)
        {
            Altar?.NoteUnyieldingDamage(damage);
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, damage, Owner, null);
        }

        var emptying = Amount <= 1;
        await PowerCmd.Decrement(this);
        if (emptying)
            Altar?.StunUnyielding();
    }
}
