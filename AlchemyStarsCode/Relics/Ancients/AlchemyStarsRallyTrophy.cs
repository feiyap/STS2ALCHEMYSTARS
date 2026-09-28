using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 拉力奖杯：若当前房间为事件房，战斗开始时将敌人生命设为约一半。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsRallyTrophy : AlchemyStarsAncientRelicBase
{
    public override async Task BeforeCombatStart()
    {
        if (Owner == null)
            return;

        var room = Owner.RunState.BaseRoom ?? Owner.RunState.CurrentRoom;
        if (room is not EventRoom)
            return;

        var combatState = Owner.Creature.CombatState;
        if (combatState == null)
            return;

        Flash();
        foreach (var enemy in combatState.HittableEnemies.ToList())
        {
            var halfHp = Math.Max(1m, Math.Ceiling(enemy.MaxHp / 2m));
            await CreatureCmd.SetCurrentHp(enemy, halfHp);
        }
    }
}
