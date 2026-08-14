using System.Threading.Tasks;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 蒂娜：层数表示回手倒计时（固定 2）；另计深色格剩余次数（1/2）。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsTinaTurnStartPower : ModPowerTemplate
{
    private CardModel? _exhaustedCard;
    private int _darkCellsRemaining;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 登记待回手的消耗卡牌，以及接下来若干回合开始时获得水深色格的次数。
    /// </summary>
    public void ConfigureExhaustedCard(CardModel card, int darkCellTurns)
    {
        _exhaustedCard = card;
        _darkCellsRemaining = darkCellTurns;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner || Amount <= 0)
            return;

        if (_darkCellsRemaining > 0)
        {
            LightMechanic.TryAddAttributeCell(player, LightElement.Water, AttributeCellKind.Dark);
            _darkCellsRemaining--;
            Flash();
        }

        var isLastStack = Amount <= 1;
        await PowerCmd.Decrement(this);

        if (!isLastStack || _exhaustedCard == null)
            return;

        if (_exhaustedCard.Pile?.Type == PileType.Exhaust)
            await CardPileCmd.Add(_exhaustedCard, PileType.Hand);
    }
}
