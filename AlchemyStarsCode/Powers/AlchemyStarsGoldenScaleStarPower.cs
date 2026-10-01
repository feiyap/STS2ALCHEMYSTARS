using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 灿星天秤：胜利时，能力持有者获得金币，并按已损失生命百分比治疗。
/// 卡牌会给自己与目标各挂一份本能力。必须用 AfterCombatEnd：引擎会在 AfterCombatVictory 前清掉能力。
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
        var player = Owner.Player;
        if (player == null)
            return;

        Flash();
        await PlayerCmd.GainGold(VictoryGold, player);

        var heal = (Owner.MaxHp - Owner.CurrentHp) * _lostHpHealPercent;
        if (heal <= 0m)
            return;

        await CreatureCmd.Heal(Owner, heal);
    }
}
