using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 莫比乌斯雷连击：接下来若干张攻击牌额外自动打出一次。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsMobiusExtraHitPower : AlchemyStarsPowerBase
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    private bool _echoing;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_echoing || Owner == null || cardPlay.Card.Owner?.Creature != Owner)
            return;

        if (cardPlay.Card.Type != CardType.Attack || Amount <= 0)
            return;

        Amount -= 1;
        _echoing = true;
        try
        {
            await CardCmd.AutoPlay(choiceContext, cardPlay.Card, cardPlay.Target);
        }
        finally
        {
            _echoing = false;
        }

        if (Amount <= 0)
            await PowerCmd.Remove(this);
    }
}
