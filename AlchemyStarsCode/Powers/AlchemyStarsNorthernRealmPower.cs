using System.Collections.Generic;
using System.Threading.Tasks;
using AlchemyStars.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace AlchemyStars.Powers;

/// <summary>
/// 北境之力：本场战斗中，每张水属性牌首次打出时获得 1 次重放。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsNorthernRealmPower : ModPowerTemplate
{
    private static readonly AttachedState<CardModel, bool> ReplayGranted = new(_ => false);

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<string> RegisteredKeywordIds => ["northern_realm"];

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner)
            return Task.CompletedTask;

        if (!AlchemyStarsCardHelpers.HasWaterKeyword(cardPlay.Card))
            return Task.CompletedTask;

        if (ReplayGranted[cardPlay.Card])
            return Task.CompletedTask;

        ReplayGranted[cardPlay.Card] = true;
        cardPlay.Card.BaseReplayCount += 1;
        Flash();
        return Task.CompletedTask;
    }
}
