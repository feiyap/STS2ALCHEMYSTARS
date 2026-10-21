using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Monsters.Powers;

/// <summary>
/// 开辟者：最大生命减半。击破解封或击败祭剑座时把因此失去的上限加回去。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsPioneerPower : AlchemyStarsSwordAltarPowerBase
{
    private int _lostMaxHp;

    protected override string PlaceholderIcon => "AlchemyStarsPioneer";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override int DisplayAmount => 0;

    public override bool ShouldScaleInMultiplayer => false;

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        var lose = Owner.MaxHp - System.Math.Max(1, Owner.MaxHp / 2);
        if (lose <= 0)
            return;

        AssertMutable();
        _lostMaxHp = lose;
        await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), Owner, lose, false);
    }

    public override Task AfterRemoved(Creature oldOwner) => Refund(oldOwner);

    /// <summary>
    /// 战斗结束清能力时不一定走 AfterRemoved，这里再补一次。已经返还过的不会再加。
    /// </summary>
    public override Task AfterCombatEnd(CombatRoom room) => Refund(Owner);

    /// <summary>祭剑座被击败时调用。随后移除能力不会再次返还。</summary>
    public Task RefundNow() => Refund(Owner);

    private async Task Refund(Creature creature)
    {
        if (_lostMaxHp <= 0)
            return;

        var restored = creature.MaxHp + _lostMaxHp;
        _lostMaxHp = 0;
        await CreatureCmd.SetMaxHp(creature, restored);
    }
}
