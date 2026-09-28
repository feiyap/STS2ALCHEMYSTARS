using System.Threading.Tasks;
using AlchemyStars.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 接下来若干张雷属性牌各额外打出 1 次。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsThunderExtraPlayPower : AlchemyStarsPowerBase
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        if (Amount <= 0 || card.Owner.Creature != Owner)
            return playCount;

        if (!AlchemyStarsCardHelpers.HasThunderKeyword(card))
            return playCount;

        return playCount + 1;
    }

    public override async Task AfterModifyingCardPlayCount(CardModel card)
    {
        if (card.Owner.Creature != Owner || !AlchemyStarsCardHelpers.HasThunderKeyword(card))
            return;

        Flash();
        if (Amount <= 1m)
            await PowerCmd.Remove(this);
        else
            await PowerCmd.Decrement(this);
    }
}
