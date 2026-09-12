using AlchemyStars.Keywords;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 启迪者锁属性展示牌：状态牌，卡图按先古样式展示。
/// </summary>
public abstract class AlchemyStarsEnlightenerChoiceCardBase : ModCardTemplate, IAncientCardArtStyle
{
    private const int BaseEnergyCost = -1;
    private const CardType CardKind = CardType.Status;
    private const CardRarity CardRarityValue = CardRarity.Status;
    private const TargetType CardTarget = TargetType.None;
    private const bool ShowInCardLibrary = false;

    protected abstract string AttributeKeywordId { get; }

    public override bool CanBeGeneratedInCombat => false;

    public override CardPoolModel VisualCardPool => ModelDb.CardPool<ColorlessCardPool>();

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Unplayable,
        ModKeywordRegistry.GetCardKeyword(AttributeKeywordId),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AttributeKeywordId)),
    ];

    protected AlchemyStarsEnlightenerChoiceCardBase()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }
}
