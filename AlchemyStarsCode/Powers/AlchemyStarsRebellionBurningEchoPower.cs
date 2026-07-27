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
/// 反叛灼燃·莱因哈特：回合末对全体敌人造成已损失生�?25% 伤害�?/// </summary>
[RegisterPower]
public sealed class AlchemyStarsRebellionBurningEchoPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

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

        var enemies = Owner.CombatState!.HittableEnemies.ToList();
        foreach (var enemy in enemies)
        {
            var missingHp = enemy.MaxHp - enemy.CurrentHp;
            if (missingHp <= 0m)
                continue;

            var damage = missingHp * 0.25m;
            using (LightMechanicDamageContext.Use(LightElement.Prismatic))
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
