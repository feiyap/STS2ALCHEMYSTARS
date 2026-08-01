using System.Collections.Generic;
using System.Threading.Tasks;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 卡莲：回合结束时按转色栏水属性格数量获得格挡�?
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsKarenGuardPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(6m, ValueProp.Move)
    ];

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

        var waterCells = LightMechanic.CountEffectiveWaterCells(player);
        var block = waterCells * DynamicVars.Block.BaseValue;
        if (block > 0)
            await CreatureCmd.GainBlock(Owner, new BlockVar(block, ValueProp.Move), null);

        await PowerCmd.Remove(this);
    }
}
