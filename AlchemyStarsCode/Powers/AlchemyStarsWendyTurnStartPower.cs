using System.Collections.Generic;
using System.Threading.Tasks;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 飓风灵鸮：下个回合开始时额外获得 1 点能量与 1 点森属性光能。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsWendyTurnStartPower : AlchemyStarsPowerBase
{
    private const int BonusEnergy = 1;
    private const int BonusForestLightEnergy = 1;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(BonusEnergy)
    ];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
            return;

        await PlayerCmd.GainEnergy(BonusEnergy, player);
        LightMechanic.TryGrantLightEnergyMany(player, LightElement.Forest, BonusForestLightEnergy);
        await PowerCmd.Remove(this);
    }
}
