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
/// 查莉娅：每回合开始时，将 1 个属性格转化为水属性深色格；未完成转色则获得能量并抽牌。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsCharlotteConvertPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
            return;

        var state = LightMechanic.GetActiveState(player);
        if (state == null)
            return;

        var cells = state.AttributeCells.Items.ToList();
        if (cells.Count == 0)
        {
            await PlayerCmd.GainEnergy(1, player);
            await CardPileCmd.Draw(choiceContext, 1, player);
            return;
        }

        // 优先转化非水属性格；若无则转化普通水格（尚未深色）。
        var convertIndex = -1;
        for (var i = 0; i < cells.Count; i++)
        {
            if (cells[i].Element != LightElement.Water)
            {
                convertIndex = i;
                break;
            }
        }

        if (convertIndex < 0)
        {
            for (var i = 0; i < cells.Count; i++)
            {
                if (cells[i].Element == LightElement.Water && cells[i].Kind != AttributeCellKind.Dark)
                {
                    convertIndex = i;
                    break;
                }
            }
        }

        if (convertIndex < 0)
        {
            await PlayerCmd.GainEnergy(1, player);
            await CardPileCmd.Draw(choiceContext, 1, player);
            return;
        }

        var source = cells[convertIndex];
        cells[convertIndex] = new AttributeCell(LightElement.Water, AttributeCellKind.Dark, source.EnhancedCardTypeName);
        state.AttributeCells.ReplaceAll(cells);
        LightMechanicUiBootstrap.RefreshForPlayer(player);
        Flash();
    }
}
