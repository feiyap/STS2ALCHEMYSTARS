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
/// 缘木求叶·尤拉：获得 1/2 森光能；将非森格转为森格（大概率强化）；下回合抽 1/2 张。
/// </summary>
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

    /// <summary>大概率赋予强化格。</summary>
    private const int EnhancedChancePercent = 60;

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
    ];

    public AlchemyStarsForestCommon4()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await AlchemyStarsCardHelpers.TriggerSkillCastAnim(this);

        var energyGain = DynamicVars["ForestLightGain"].IntValue;
        LightMechanic.TryGrantLightEnergyMany(Owner, LightElement.Forest, energyGain);
        // 仅转化非森格为森格，大概率强化。
        LightMechanic.ResetNonForestCells(
            Owner,
            normalChancePercent: 100 - EnhancedChancePercent,
            enhancedChancePercent: EnhancedChancePercent);

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
