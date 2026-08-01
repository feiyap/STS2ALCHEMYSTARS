using System.Collections.Generic;
using System.Threading.Tasks;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Models;
using AlchemyStars.Keywords;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 金泽之星：胜利获得金币，下场战斗开始时获得雷属性棱镜格�?/// </summary>
[RegisterPower]
public sealed class AlchemyStarsGoldenScaleStarPower : ModPowerTemplate
{
    private const decimal VictoryGold = 30m;
    private const int PrismCellCount = 2;

    private bool _grantPrismNextCombat;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<string> RegisteredKeywordIds => [AlchemyStarsKeywordIds.GoldenScaleStar];

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        var player = Owner.Player;
        if (player == null)
            return;

        Flash();
        await PlayerCmd.GainGold(VictoryGold, player);
        _grantPrismNextCombat = true;
    }

    public override async Task BeforeCombatStart()
    {
        if (!_grantPrismNextCombat)
            return;

        var player = Owner.Player;
        if (player == null || !LightMechanic.HasMechanicRelic(player))
            return;

        _grantPrismNextCombat = false;
        for (var i = 0; i < PrismCellCount; i++)
            LightMechanic.TryAddAttributeCell(player, LightElement.Thunder, AttributeCellKind.Prism);
    }
}
