using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using AlchemyStars.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 缘木求叶·尤拉：获得森光能并重置转色栏，下回合开始时抽牌�?/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsForestCommon4 : ModCardTemplate
{
    private const int BaseEnergyCost = 0;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Common;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;
    private const int BaseForestEnergyGain = 1;
    private const int ForestEnergyGainUpgradeBy = 1;
    private const int BaseDrawCount = 1;
    private const int DrawCountUpgradeBy = 1;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("ForestLightGain", BaseForestEnergyGain),
        new CardsVar(BaseDrawCount),
        AlchemyStarsKeywordText.InlineTitleVar("ForestTitle", AlchemyStarsKeywordIds.Forest)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.LightEnergy)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.AttributeCell))
    ];

    public AlchemyStarsForestCommon4()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var energyGain = DynamicVars["ForestLightGain"].IntValue;
        LightMechanic.TryGrantLightEnergyMany(Owner, LightElement.Forest, energyGain);
        LightMechanic.ResetAllCellsWithEnhanced(Owner, LightElement.Forest);

        var drawCount = DynamicVars.Cards.IntValue;
        var drawPower = await PowerCmd.Apply<AlchemyStarsYuraDrawPower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            this);

        drawPower?.Configure(drawCount);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ForestLightGain"].UpgradeValueBy(ForestEnergyGainUpgradeBy);
        DynamicVars.Cards.UpgradeValueBy(DrawCountUpgradeBy);
    }
}
