using AlchemyStars.Keywords;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 万门之钥回忆事件选项卡：仅作选卡展示，不进牌库、不可打出。
/// </summary>
public abstract class AlchemyStarsMemoryDoorChoiceCardBase : ModCardTemplate
{
    private const int BaseEnergyCost = -1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Status;
    private const TargetType CardTarget = TargetType.None;
    private const bool ShowInCardLibrary = false;

    /// <summary>选中后进入的回忆事件类型。</summary>
    public abstract Type TargetEventType { get; }

    protected abstract IReadOnlyList<string> AttributeKeywordIds { get; }

    public override bool CanBeGeneratedInCombat => false;

    public override CardPoolModel VisualCardPool => ModelDb.CardPool<ColorlessCardPool>();

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        AttributeKeywordIds
            .Select(ModKeywordRegistry.GetCardKeyword)
            .Prepend(CardKeyword.Unplayable);

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        AttributeKeywordIds.Select(id =>
            HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(id)));

    protected AlchemyStarsMemoryDoorChoiceCardBase()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }
}
