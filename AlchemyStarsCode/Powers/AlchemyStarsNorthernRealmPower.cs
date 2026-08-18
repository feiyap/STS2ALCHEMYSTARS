using System.Threading.Tasks;
using AlchemyStars.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace AlchemyStars.Powers;

/// <summary>
/// 北境之力：本场战斗中，每张水属性牌首次打出时重放 1。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsNorthernRealmPower : ModPowerTemplate
{
    private static readonly AttachedState<CardModel, bool> HasReplayedOnFirstPlay = new(_ => false);

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        if (card.Owner.Creature != Owner)
            return playCount;

        if (!AlchemyStarsCardHelpers.HasWaterKeyword(card))
            return playCount;

        if (HasReplayedOnFirstPlay[card])
            return playCount;

        HasReplayedOnFirstPlay[card] = true;
        return playCount + 1;
    }

    public override Task AfterModifyingCardPlayCount(CardModel card)
    {
        Flash();
        return Task.CompletedTask;
    }
}
