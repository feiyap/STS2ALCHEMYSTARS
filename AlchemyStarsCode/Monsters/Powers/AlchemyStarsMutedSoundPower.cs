using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Monsters.Powers;

/// <summary>
/// 哑音：每名玩家每打出 3 张牌，随机消耗 1 张手牌。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsMutedSoundPower : AlchemyStarsSwordAltarPowerBase
{
    private readonly Dictionary<Player, int> _cardsPlayed = new();

    protected override string PlaceholderIcon => "AlchemyStarsMutedSoundPower";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override int DisplayAmount => 0;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        if (player == null)
            return;

        AssertMutable();
        _cardsPlayed.TryGetValue(player, out var played);
        played++;
        if (played < 3)
        {
            _cardsPlayed[player] = played;
            return;
        }

        _cardsPlayed[player] = 0;
        var hand = PileType.Hand.GetPile(player).Cards.ToList();
        if (hand.Count == 0)
            return;

        var card = player.RunState.Rng.CombatCardSelection.NextItem(hand);
        if (card != null)
            await CardCmd.Exhaust(choiceContext, card);
    }
}
