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
/// 查莉娅：每回合开始时，将雷与森属性格转化为水属性深色格；未完成转色则获得能量并抽牌�?/// </summary>
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
        var convertedAny = false;
        for (var i = 0; i < cells.Count; i++)
        {
            var cell = cells[i];
            if (cell.Element is not (LightElement.Thunder or LightElement.Forest))
                continue;

            cells[i] = new AttributeCell(LightElement.Water, AttributeCellKind.Dark, cell.EnhancedCardTypeName);
            convertedAny = true;
        }

        if (convertedAny)
        {
            state.AttributeCells.ReplaceAll(cells);
            LightMechanicUiBootstrap.RefreshForPlayer(player);
            return;
        }

        await PlayerCmd.GainEnergy(1, player);
        await CardPileCmd.Draw(choiceContext, 1, player);
    }
}
