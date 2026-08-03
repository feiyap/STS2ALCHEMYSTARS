using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlchemyStars.Cards;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 结晶领域：手牌中时，未格挡的森属性攻击施加结晶�?/// </summary>
[RegisterPower]
public sealed class AlchemyStarsCrystallizationDomainPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<string> RegisteredKeywordIds => [AlchemyStarsKeywordIds.Crystallization];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player)
            return;

        if (cardPlay.Card.Type != CardType.Attack || !AlchemyStarsCardHelpers.HasForestKeyword(cardPlay.Card))
            return;

        if (cardPlay.Target == null || cardPlay.Target.IsDead)
            return;

        if (cardPlay.Target.Block > 0)
            return;

        await PowerCmd.Apply<AlchemyStarsCrystallizationPower>(
            choiceContext,
            cardPlay.Target,
            1m,
            Owner,
            cardPlay.Card);
    }
}
