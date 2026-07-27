using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 蛮牛蜃影：回合开始时对全体敌人造成森属性伤害�?/// </summary>
[RegisterPower]
public sealed class AlchemyStarsMilosBarragePower : ModPowerTemplate
{
    private decimal _damage = 5m;
    private int _turnsRemaining;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public void Configure(decimal damage, int turns)
    {
        _damage = damage;
        _turnsRemaining = turns;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner || _turnsRemaining <= 0)
            return;

        var enemies = Owner.CombatState!.HittableEnemies.ToList();
        foreach (var enemy in enemies)
        {
            await LightMechanic.DealElementalAttackDamage(
                choiceContext,
                player,
                null,
                enemy,
                _damage,
                LightElement.Forest);
        }

        _turnsRemaining--;
        if (_turnsRemaining <= 0)
            await PowerCmd.Remove(this);
    }
}
