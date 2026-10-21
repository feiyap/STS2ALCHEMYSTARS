using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 雷光闪击·艾莉法：非雷格转为雷格，获得格挡，并获得 1/2 点雷光能。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsThunderCommon6 : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Common;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    public override bool GainsBlock => true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(6m, ValueProp.Move),
        new IntVar("ThunderGain", 1),
        AlchemyStarsKeywordText.InlineTitleVar("ThunderTitle", AlchemyStarsKeywordIds.Thunder)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder))
    ];

    public AlchemyStarsThunderCommon6() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await AlchemyStarsCardHelpers.TriggerSkillCastAnim(this);

        // 非雷格全部转为雷格（不生成深色格）。
        LightMechanic.TryConvertAllCellsToElement(Owner, LightElement.Thunder, darkChancePercent: 0);

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        LightMechanic.TryGrantLightEnergyMany(
            Owner,
            LightElement.Thunder,
            DynamicVars["ThunderGain"].IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars["ThunderGain"].UpgradeValueBy(1m);
    }
}
