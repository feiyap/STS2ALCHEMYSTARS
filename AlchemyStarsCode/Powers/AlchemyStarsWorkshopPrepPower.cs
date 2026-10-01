using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 工备：回合结束时每层造成 1 点雷属性伤害并获得同额格挡；每 3 层获 1 点雷属性光能。
/// 与炸弹相同，每次施加为独立实例，可同时存在多份工备。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsWorkshopPrepPower : AlchemyStarsPowerBase
{
    private const int LightEnergyInterval = 3;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 每次打出能力卡获得工备时新建独立实例（同炸弹）。
    /// </summary>
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
            return;

        Flash();
        await PowerCmd.ModifyAmount(choiceContext, this, 1m, Owner, null);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner) || Amount <= 0)
            return;

        var player = Owner.Player;
        if (player == null)
            return;

        var stacks = (int)Amount;
        var enemies = Owner.CombatState!.HittableEnemies.ToList();
        if (enemies.Count > 0)
        {
            for (var i = 0; i < stacks; i++)
            {
                var target = player.RunState.Rng.CombatTargets.NextItem(enemies);
                if (target == null || target.IsDead)
                    continue;

                await LightMechanic.DealElementalAttackDamage(
                    choiceContext,
                    player,
                    null,
                    target,
                    1m,
                    LightElement.Thunder);
            }
        }

        await CreatureCmd.GainBlock(Owner, new BlockVar(stacks, ValueProp.Move), null);

        var lightEnergyGain = stacks / LightEnergyInterval;
        if (lightEnergyGain > 0)
            LightMechanic.TryGrantLightEnergyMany(player, LightElement.Thunder, lightEnergyGain);
    }
}
