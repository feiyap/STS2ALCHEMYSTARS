using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 幻象双刃·菲莉诗：每份在回合开始时治疗全体队友已损失生命的 4%；升级份额外治疗最大生命的 4%。
/// Amount = 份数。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsFeliciaHealPower : AlchemyStarsPowerBase
{
    private const decimal LostHpHealPercent = 0.04m;
    private const decimal MaxHpHealPercent = 0.04m;

    private int _upgradedCopies;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 升级份额外按最大生命百分比治疗。只在 value 为 true 时累加份数，避免第二张未升级覆盖第一张升级。
    /// </summary>
    public void ConfigureAlsoHealMaxHpPercent(bool value)
    {
        if (value)
            _upgradedCopies++;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner || Owner.CombatState == null)
            return;

        var copies = (int)Amount;
        if (copies <= 0)
            return;

        Flash();

        var allies = Owner.CombatState.PlayerCreatures
            .Where(creature => creature.IsAlive && creature.IsPlayer)
            .ToList();

        foreach (var ally in allies)
        {
            var heal = (ally.MaxHp - ally.CurrentHp) * LostHpHealPercent * copies;
            if (_upgradedCopies > 0)
                heal += ally.MaxHp * MaxHpHealPercent * _upgradedCopies;

            if (heal <= 0m)
                continue;

            await CreatureCmd.Heal(ally, heal);
        }
    }
}
