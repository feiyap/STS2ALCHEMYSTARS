using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using AlchemyStars.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 惑羽绝尘·耶利亚：言绝；手牌中吸收强化格降低费用，打出后依费用差获得飞行�?/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsForestRare3 : ModCardTemplate
{
    private const int BaseEnergyCost = 7;
    private const CardType CardKind = CardType.Power;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<AlchemyStarsWordAbsolutePower>(1m),
        AlchemyStarsKeywordText.InlineTitleVar("WordAbsolute", AlchemyStarsKeywordIds.WordAbsolute),
        AlchemyStarsKeywordText.InlineTitleVar("ForestTitle", AlchemyStarsKeywordIds.Forest)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.WordAbsolute)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.WordAbsolute)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest)),
        HoverTipFactory.FromPower<AlchemyStarsFlyingPower>(),
        HoverTipFactory.FromPower<AlchemyStarsWordAbsolutePower>()
    ];

    public AlchemyStarsForestRare3()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    public override bool TryModifyEnergyCostInCombat(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (!ReferenceEquals(card, this))
            return false;

        if (AlchemyStarsForestState.GetWordAbsoluteInitialCost(this) <= 0)
        {
            AlchemyStarsForestState.SetWordAbsoluteInitialCost(
                this,
                (int)originalCost);
        }

        var reduction = AlchemyStarsForestState.GetWordAbsoluteCostReduction(this);
        if (reduction <= 0)
            return false;

        modifiedCost = Math.Max(0m, originalCost - reduction);
        return modifiedCost != originalCost;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (AlchemyStarsForestState.GetWordAbsoluteInitialCost(this) <= 0)
        {
            AlchemyStarsForestState.SetWordAbsoluteInitialCost(
                this,
                EnergyCost.GetWithModifiers(CostModifiers.All));
        }

        var initialCost = AlchemyStarsForestState.GetWordAbsoluteInitialCost(this);
        var currentCost = EnergyCost.GetWithModifiers(CostModifiers.All);
        var flyingGain = Math.Max(0, initialCost - currentCost);
        if (flyingGain > 0)
        {
            await PowerCmd.Apply<AlchemyStarsFlyingPower>(
                choiceContext,
                Owner.Creature,
                flyingGain,
                Owner.Creature,
                this);
        }

        await PowerCmd.Apply<AlchemyStarsWordAbsolutePower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["AlchemyStarsWordAbsolutePower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-2);
    }
}
