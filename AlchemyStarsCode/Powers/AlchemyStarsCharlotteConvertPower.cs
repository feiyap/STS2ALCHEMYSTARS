using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 查莉娅：每份在回合开始时将 1 格转为水深色格；未能转色的每份改为获得能量并抽 1 张牌。
/// Amount = 份数。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsCharlotteConvertPower : AlchemyStarsPowerBase
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
            return;

        var copies = (int)Amount;
        if (copies <= 0)
            return;

        var state = LightMechanic.GetActiveState(player);
        if (state == null)
            return;

        var cells = state.AttributeCells.Items.ToList();
        var converted = 0;
        for (var n = 0; n < copies; n++)
        {
            var convertIndex = FindConvertibleIndex(cells);
            if (convertIndex < 0)
                break;

            var source = cells[convertIndex];
            cells[convertIndex] = new AttributeCell(LightElement.Water, AttributeCellKind.Dark, source.EnhancedCardTypeName);
            converted++;
        }

        if (converted > 0)
        {
            state.AttributeCells.ReplaceAll(cells);
            LightMechanicUiBootstrap.RefreshForPlayer(player);
            Flash();
        }

        var failed = copies - converted;
        if (failed <= 0)
            return;

        await PlayerCmd.GainEnergy(failed, player);
        await CardPileCmd.Draw(choiceContext, failed, player);
        if (converted == 0)
            Flash();
    }

    private static int FindConvertibleIndex(IReadOnlyList<AttributeCell> cells)
    {
        for (var i = 0; i < cells.Count; i++)
        {
            if (cells[i].Element != LightElement.Water)
                return i;
        }

        for (var i = 0; i < cells.Count; i++)
        {
            if (cells[i].Element == LightElement.Water && cells[i].Kind != AttributeCellKind.Dark)
                return i;
        }

        return -1;
    }
}
