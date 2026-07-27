using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 灿烂雨露·拉斐尔：高庭卫队；手牌另有高庭卫队时获能，获得水光能并按水属性格数量治疗�?/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsWaterUncommon1 : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;
    private const int WaterEnergyGain = 2;
    private const int HealPerWaterCell = 1;
    private const int UpgradedHealPerWaterCell = 2;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1),
        new HealVar(HealPerWaterCell),
        AlchemyStarsKeywordText.InlineTitleVar("HighCourtGuard", AlchemyStarsKeywordIds.HighCourtGuard),
        AlchemyStarsKeywordText.InlineTitleVar("WaterTitle", AlchemyStarsKeywordIds.Water)
    ];

    protected override HashSet<CardTag> CanonicalTags => [AlchemyStarsCardTags.HighCourtGuard];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.HighCourtGuard)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.HighCourtGuard)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.LightEnergy)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.AttributeCell))
    ];

    public AlchemyStarsWaterUncommon1()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (AlchemyStarsCardHelpers.HasOtherTagInHand(this, Owner, AlchemyStarsCardTags.HighCourtGuard))
            await PlayerCmd.GainEnergy(1, Owner);

        LightMechanic.TryGrantLightEnergyMany(Owner, LightElement.Water, WaterEnergyGain);

        var waterCells = LightMechanic.CountWaterAttributeCells(Owner);
        var heal = waterCells * DynamicVars.Heal.IntValue;
        if (heal > 0)
            await CreatureCmd.Heal(Owner.Creature, heal);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Heal.UpgradeValueBy(UpgradedHealPerWaterCell - HealPerWaterCell);
    }
}
