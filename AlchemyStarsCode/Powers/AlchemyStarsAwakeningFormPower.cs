using AlchemyStars.Cards;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 觉醒形态：每打出 15 张属性牌，用万色格填满转色栏。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsAwakeningFormPower : AlchemyStarsPowerBase
{
    private const int AttributeCardsPerTrigger = 15;

    private int _pendingAttributePlays;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != Owner)
            return;

        if (!AlchemyStarsCardHelpers.IsAttributeCard(cardPlay.Card))
            return;

        _pendingAttributePlays++;
        while (_pendingAttributePlays >= AttributeCardsPerTrigger)
        {
            _pendingAttributePlays -= AttributeCardsPerTrigger;
            Flash();
            var player = Owner.Player;
            if (player != null)
                LightMechanic.FillAttributeBarWithPrismatic(player);
        }

        await Task.CompletedTask;
    }
}
