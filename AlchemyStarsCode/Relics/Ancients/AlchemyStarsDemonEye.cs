using AlchemyStars.Characters;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Ancients;

/// <summary>
/// 恶魔之眼：每回合获得 1 能量并抽 1 张；
/// 若本回合首张打出的牌类型与上一场/上回合最后打出的类型相同，则往抽牌堆加入 2 张晕眩。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsDemonEye : AlchemyStarsAncientRelicBase
{
    private const int NoType = -1;

    private int _lastPlayedType = NoType;
    private CardType? _firstPlayedTypeThisTurn;
    private bool _checkedFirstCardThisTurn;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

    protected override bool IncludeEnergyHoverTip => true;

    /// <summary>
    /// 上一场/上回合最后打出的牌类型（CardType 的整型值，-1 表示无）。
    /// </summary>
    [SavedProperty]
    public int LastPlayedType
    {
        get => _lastPlayedType;
        set
        {
            AssertMutable();
            _lastPlayedType = value;
        }
    }

    public override async Task AfterEnergyReset(Player player)
    {
        if (player != Owner)
            return;

        Flash();
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
        await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), Owner);
    }

    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (Owner != null && participants.Contains(Owner.Creature))
        {
            _firstPlayedTypeThisTurn = null;
            _checkedFirstCardThisTurn = false;
        }

        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || cardPlay.Card.Owner != Owner)
            return;

        if (!CombatManager.Instance.IsInProgress)
            return;

        // 记录本回合首张打出的类型，并与历史类型比对。
        if (!_checkedFirstCardThisTurn)
        {
            _checkedFirstCardThisTurn = true;
            _firstPlayedTypeThisTurn = cardPlay.Card.Type;

            if (LastPlayedType != NoType && (CardType)LastPlayedType == cardPlay.Card.Type)
            {
                Flash();
                await CardPileCmd.AddToCombatAndPreview<Dazed>(
                    Owner.Creature,
                    PileType.Draw,
                    2,
                    Owner);
            }
        }

        LastPlayedType = (int)cardPlay.Card.Type;
    }

    public override Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner != null && participants.Contains(Owner.Creature) && _firstPlayedTypeThisTurn.HasValue)
            LastPlayedType = (int)_firstPlayedTypeThisTurn.Value;

        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom _)
    {
        _firstPlayedTypeThisTurn = null;
        _checkedFirstCardThisTurn = false;
        return Task.CompletedTask;
    }
}
