using System.Linq;
using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Character;

/// <summary>
/// 十连召集：每场战斗第一回合抽 10 选 4，其余进弃牌堆。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsTenPullSummon : AlchemyStarsCharacterRelicBase
{
    private bool _pendingFirstTurn;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override Task BeforeCombatStart()
    {
        _pendingFirstTurn = true;
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom _)
    {
        _pendingFirstTurn = false;
        return Task.CompletedTask;
    }

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player != Owner || !_pendingFirstTurn)
            return count;
        return 10m;
    }

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner || !_pendingFirstTurn || Owner == null)
            return;

        _pendingFirstTurn = false;
        var hand = PileType.Hand.GetPile(Owner);
        if (hand.Cards.Count <= 4)
            return;

        Flash();
        var maxKeep = Math.Min(4, hand.Cards.Count);
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, maxKeep, maxKeep);
        var keep = (await CardSelectCmd.FromCombatPile(
            choiceContext,
            hand,
            Owner,
            prefs)).ToHashSet();

        foreach (var card in hand.Cards.Where(c => !keep.Contains(c)).ToList())
            await CardPileCmd.Add(card, PileType.Discard);
    }
}
