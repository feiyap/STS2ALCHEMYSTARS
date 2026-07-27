using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 静默雷霆：回合结束时再次对所有敌人造成等值雷属性伤害�?/// </summary>
[RegisterPower]
public sealed class AlchemyStarsMichelleEchoPower : ModPowerTemplate
{
    private decimal _echoDamage;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    private CardModel? _sourceCard;

    public void SetEchoDamage(decimal damage, CardModel? sourceCard)
    {
        _echoDamage = damage;
        _sourceCard = sourceCard;
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner) || _echoDamage <= 0m)
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
            await LightMechanic.DealElementalAttackDamage(
                choiceContext,
                player,
                _sourceCard,
                enemy,
                _echoDamage,
                LightElement.Thunder);
        }

        await PowerCmd.Remove(this);
    }
}
