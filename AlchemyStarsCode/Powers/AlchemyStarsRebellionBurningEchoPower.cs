using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// <summary>
/// 反叛灼燃·莱因哈特：回合末对全体敌人造成已损失生命比例火/雷伤害（层数=百分比）。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsRebellionBurningEchoPower : AlchemyStarsPowerBase
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner))
            return;

        var player = Owner.Player;
        if (player == null)
        {
            await PowerCmd.Remove(this);
            return;
        }

        var ratio = Amount > 0m ? Amount / 100m : 0.70m;
        var enemies = Owner.CombatState!.HittableEnemies.ToList();
        foreach (var enemy in enemies)
        {
            var missingHp = enemy.MaxHp - enemy.CurrentHp;
            if (missingHp <= 0m)
                continue;

            var damage = missingHp * ratio;
            using (LightMechanicDamageContext.UseFireAndThunder())
            {
                await CreatureCmd.Damage(
                    choiceContext,
                    enemy,
                    damage,
                    ValueProp.Unblockable | ValueProp.Unpowered,
                    null,
                    null);
            }
        }

        await PowerCmd.Remove(this);
    }
}
