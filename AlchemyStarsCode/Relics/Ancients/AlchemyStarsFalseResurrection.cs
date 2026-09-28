using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 虚假复活：拾起时移除卡组全部消耗词条并加入愚蠢；每次洗牌往抽牌堆加入笨拙。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsFalseResurrection : AlchemyStarsAncientRelicBase
{
    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        if (Owner == null)
            return;

        foreach (var card in PileType.Deck.GetPile(Owner).Cards.ToList())
        {
            if (card.Keywords.Contains(CardKeyword.Exhaust))
                CardCmd.RemoveKeyword(card, CardKeyword.Exhaust);
        }

        await CardPileCmd.AddCurseToDeck<Folly>(Owner);
        Flash();
    }

    public override async Task AfterShuffle(PlayerChoiceContext choiceContext, Player shuffler)
    {
        if (shuffler != Owner || Owner == null)
            return;

        Flash();
        await CardPileCmd.AddToCombatAndPreview<Clumsy>(
            Owner.Creature,
            PileType.Draw,
            1,
            Owner);
    }
}
