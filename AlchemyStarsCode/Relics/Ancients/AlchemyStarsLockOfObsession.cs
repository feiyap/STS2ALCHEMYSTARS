using System.Linq;
using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 妄执之锁：不自动洗牌；每回合自选弃牌堆至多 4 张进抽牌堆；一旦洗牌则本场战斗失效。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsLockOfObsession : AlchemyStarsAncientRelicBase
{
    private const int MaxPickPerTurn = 4;

    private bool _broken;
    private int _pickedThisTurn;

    public override bool IsUsedUp => _broken;

    /// <summary>本场战斗是否已因洗牌失效（跨战斗会重置）。</summary>
    [SavedProperty]
    public bool Broken
    {
        get => _broken;
        set
        {
            AssertMutable();
            _broken = value;
            Status = value ? RelicStatus.Disabled : RelicStatus.Active;
        }
    }

    public bool IsActive => Owner != null && !Broken;

    public override Task BeforeCombatStart()
    {
        Broken = false;
        _pickedThisTurn = 0;
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom _)
    {
        Broken = false;
        _pickedThisTurn = 0;
        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (IsActive && Owner != null && participants.Contains(Owner.Creature))
            _pickedThisTurn = 0;
        return Task.CompletedTask;
    }

    public override Task AfterShuffle(PlayerChoiceContext choiceContext, Player shuffler)
    {
        if (shuffler == Owner && IsActive)
        {
            Flash();
            Broken = true;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 替代 <see cref="CardPileCmd.ShuffleIfNecessary"/>：自选弃牌放入抽牌堆顶。
    /// </summary>
    public async Task ReplaceShuffleIfNecessaryAsync(PlayerChoiceContext choiceContext, Player player)
    {
        if (!IsActive || player != Owner || Owner == null)
            return;

        var draw = PileType.Draw.GetPile(Owner);
        var discard = PileType.Discard.GetPile(Owner);
        if (draw.Cards.Count > 0 || discard.Cards.Count == 0)
            return;

        var remaining = MaxPickPerTurn - _pickedThisTurn;
        if (remaining <= 0)
            return;

        var maxPick = Math.Min(remaining, discard.Cards.Count);
        Flash();
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1, maxPick);
        var selected = (await CardSelectCmd.FromCombatPile(
            choiceContext,
            discard,
            Owner,
            prefs)).ToList();

        // 先选的先抽：逆序放到抽牌堆顶。
        for (var i = selected.Count - 1; i >= 0; i--)
            await CardPileCmd.Add(selected[i], draw, CardPilePosition.Top);

        _pickedThisTurn += selected.Count;
    }
}
