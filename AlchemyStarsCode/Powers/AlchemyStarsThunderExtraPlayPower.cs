using System.Collections.Generic;
using System.Threading.Tasks;
using AlchemyStars.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 下一张雷属性牌额外打出若干次�?/// </summary>
[RegisterPower]
public sealed class AlchemyStarsThunderExtraPlayPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (Amount <= 0 || cardPlay.Card.Owner.Creature != Owner)
            return;

        if (!AlchemyStarsCardHelpers.HasThunderKeyword(cardPlay.Card))
            return;

        Flash();
        cardPlay.Card.BaseReplayCount += (int)Amount;
        await PowerCmd.Remove(this);
    }
}
