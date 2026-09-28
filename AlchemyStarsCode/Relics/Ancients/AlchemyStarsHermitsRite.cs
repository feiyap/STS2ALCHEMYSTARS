using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 隐士仪式：拾起时从先古卡中选 1 张加入卡组；每回合第 4 张打出的牌消耗。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsHermitsRite : AlchemyStarsAncientRelicBase
{
    private int _cardsPlayedThisTurn;

    public override bool HasUponPickupEffect => true;

    public override bool ShowCounter => CombatManager.Instance.IsInProgress;

    public override int DisplayAmount => _cardsPlayedThisTurn;

    public override async Task AfterObtained()
    {
        if (Owner == null)
            return;

        var ancients = Owner.Character.CardPool
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Where(c => c.Rarity == CardRarity.Ancient)
            .Concat(ModelDb.CardPool<ColorlessCardPool>()
                .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
                .Where(c => c.Rarity == CardRarity.Ancient))
            .Select(c => Owner.RunState.CreateCard(c, Owner))
            .ToList();

        if (ancients.Count == 0)
            return;

        var prefs = new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1);
        var picked = (await CardSelectCmd.FromSimpleGrid(
            new BlockingPlayerChoiceContext(),
            ancients,
            Owner,
            prefs)).FirstOrDefault();

        if (picked == null)
            picked = ancients[0];

        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(picked, PileType.Deck));
        Flash();
    }

    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (Owner != null && participants.Contains(Owner.Creature))
        {
            _cardsPlayedThisTurn = 0;
            InvokeDisplayAmountChanged();
        }

        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || cardPlay.Card.Owner != Owner)
            return;

        if (!CombatManager.Instance.IsInProgress || cardPlay.IsAutoPlay)
            return;

        _cardsPlayedThisTurn++;
        InvokeDisplayAmountChanged();

        if (_cardsPlayedThisTurn != 4)
            return;

        Flash();
        await CardCmd.Exhaust(choiceContext, cardPlay.Card);
    }

    public override Task AfterCombatEnd(CombatRoom _)
    {
        _cardsPlayedThisTurn = 0;
        return Task.CompletedTask;
    }
}
