using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 苦恼：无法打出的诅咒。
/// </summary>
[RegisterCard(typeof(CurseCardPool))]
public sealed class AlchemyStarsAngst : ModCardTemplate
{
    private const int BaseEnergyCost = -1;
    private const CardType CardKind = CardType.Curse;
    private const CardRarity CardRarityValue = CardRarity.Curse;
    private const TargetType CardTarget = TargetType.None;
    private const bool ShowInCardLibrary = true;

    public override int MaxUpgradeLevel => 0;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/AlchemyStarsGenerated1.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Unplayable
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Unplayable)
    ];

    public AlchemyStarsAngst()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }
}
