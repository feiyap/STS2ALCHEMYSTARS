using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 幻象双刃·菲莉诗：每回合开始时治疗全体队友已损失生命的 5%；升级后额外治疗最大生命的 5%。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsFeliciaHealPower : ModPowerTemplate
{
    private const decimal LostHpHealPercent = 0.05m;
    private const decimal MaxHpHealPercent = 0.05m;

    private bool _alsoHealMaxHpPercent;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>
    /// 升级后额外按最大生命百分比治疗�?    /// </summary>
    public void ConfigureAlsoHealMaxHpPercent(bool value) => _alsoHealMaxHpPercent = value;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner || Owner.CombatState == null)
            return;

        Flash();

        var allies = Owner.CombatState.PlayerCreatures
            .Where(creature => creature.IsAlive && creature.IsPlayer)
            .ToList();

        foreach (var ally in allies)
        {
            var heal = (ally.MaxHp - ally.CurrentHp) * LostHpHealPercent;
            if (_alsoHealMaxHpPercent)
                heal += ally.MaxHp * MaxHpHealPercent;

            if (heal <= 0m)
                continue;

            await CreatureCmd.Heal(ally, heal);
        }
    }
}
