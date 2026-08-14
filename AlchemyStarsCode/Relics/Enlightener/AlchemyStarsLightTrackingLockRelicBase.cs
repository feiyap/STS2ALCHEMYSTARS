using AlchemyStars.Cards;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace AlchemyStars.Relics.Enlightener;

/// <summary>
/// 光能追踪方案 A/B：拾起时锁定一种或多种属性，过滤奖励（及可选商店）中的属性卡。
/// </summary>
public abstract class AlchemyStarsLightTrackingLockRelicBase : AlchemyStarsEnlightenerRelicBase
{
    private int _lockedAttributeMask;

    /// <summary>
    /// 是否同时过滤商店卡牌池。
    /// </summary>
    protected abstract bool AffectsShop { get; }

    public override bool HasUponPickupEffect => true;

    /// <summary>
    /// 已锁定属性的位掩码（按 <see cref="LightElement"/> 低 4 位）。
    /// </summary>
    [SavedProperty]
    public int LockedAttributeMask
    {
        get => _lockedAttributeMask;
        set
        {
            AssertMutable();
            _lockedAttributeMask = value;
        }
    }

    public bool HasLockedAttribute => LockedAttributeMask != 0;

    public override async Task AfterObtained()
    {
        if (Owner == null)
            return;

        var choices = BuildAttributeChoices(Owner);
        if (choices.Count == 0)
        {
            Entry.Logger.Warn($"[Enlightener] {GetType().Name} 未能构建属性选项，跳过锁定。");
            return;
        }

        // min != max 时引擎会 RequireManualConfirmation，需点确定确认多选。
        var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1, choices.Count)
        {
            RequireManualConfirmation = true,
        };

        var selected = (await CardSelectCmd.FromSimpleGrid(
            new BlockingPlayerChoiceContext(),
            choices,
            Owner,
            prefs)).ToList();

        if (selected.Count == 0)
            selected.Add(choices[0]);

        var locked = selected
            .Select(AttributeCardTracking.TryGetCardAttribute)
            .Where(attribute => attribute != null)
            .Select(attribute => attribute!.Value)
            .Distinct();

        LockedAttributeMask = AttributeCardTracking.ToAttributeMask(locked);
        if (LockedAttributeMask == 0)
            LockedAttributeMask = AttributeCardTracking.ToAttributeMask(LightElement.Fire);

        Flash();
    }

    public override bool TryModifyCardRewardOptionsLate(
        Player player,
        List<CardCreationResult> cardRewards,
        CardCreationOptions options)
    {
        if (player != Owner || !HasLockedAttribute)
            return false;

        if (options.Flags.HasFlag(CardCreationFlags.NoModifyHooks))
            return false;

        return AttributeCardTracking.RerollLockedRewardCards(
            player,
            cardRewards,
            options,
            LockedAttributeMask,
            this);
    }

    public override IEnumerable<CardModel> ModifyMerchantCardPool(
        Player player,
        IEnumerable<CardModel> options)
    {
        if (!AffectsShop || player != Owner || !HasLockedAttribute)
            return options;

        return options.Where(card => AttributeCardTracking.PassesAttributeLock(card, LockedAttributeMask));
    }

    private static List<CardModel> BuildAttributeChoices(Player owner) =>
    [
        owner.RunState.CreateCard<AlchemyStarsEnlightenerChoiceHeichao>(owner),
        owner.RunState.CreateCard<AlchemyStarsEnlightenerChoiceNadine>(owner),
        owner.RunState.CreateCard<AlchemyStarsEnlightenerChoiceEureka>(owner),
        owner.RunState.CreateCard<AlchemyStarsEnlightenerChoiceMuYuebai>(owner),
    ];
}
