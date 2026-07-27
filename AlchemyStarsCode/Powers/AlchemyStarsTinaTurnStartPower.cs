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
/// 蒂娜：接下来若干回合开始时，获�?1 格水属性深色格；层数耗尽后将消耗的卡牌送回手牌�?/// </summary>
[RegisterPower]
public sealed class AlchemyStarsTinaTurnStartPower : ModPowerTemplate
{
    private CardModel? _exhaustedCard;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 登记待回手的消耗卡牌�?    /// </summary>
    public void ConfigureExhaustedCard(CardModel card) => _exhaustedCard = card;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner || Amount <= 0)
            return;

        LightMechanic.TryAddAttributeCell(player, LightElement.Water, AttributeCellKind.Dark);
        Flash();

        var isLastStack = Amount <= 1;
        await PowerCmd.Decrement(this);

        if (!isLastStack || _exhaustedCard == null)
            return;

        if (_exhaustedCard.Pile?.Type == PileType.Exhaust)
            await CardPileCmd.Add(_exhaustedCard, PileType.Hand);
    }
}
