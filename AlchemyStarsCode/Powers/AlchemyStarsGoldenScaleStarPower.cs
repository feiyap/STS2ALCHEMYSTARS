using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 灿星天秤：胜利时全体玩家各得金币，并按已损失生命百分比治疗。
/// 必须用 AfterCombatEnd：引擎会在 AfterCombatVictory 前清掉能力。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsGoldenScaleStarPower : AlchemyStarsPowerBase
{
    private const decimal VictoryGold = 40m;

    private decimal _lostHpHealPercent = 0.35m;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>
    /// 由卡牌 Configure：基础 35%，升级 50%。
    /// </summary>
    public void ConfigureLostHpHealPercent(decimal percent) => _lostHpHealPercent = percent;

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        Flash();

        foreach (var player in room.CombatState.Players)
        {
            await PlayerCmd.GainGold(VictoryGold, player);

            var creature = player.Creature;
            var heal = (creature.MaxHp - creature.CurrentHp) * _lostHpHealPercent;
            if (heal <= 0m)
                continue;

            await CreatureCmd.Heal(creature, heal);
        }
    }
}
