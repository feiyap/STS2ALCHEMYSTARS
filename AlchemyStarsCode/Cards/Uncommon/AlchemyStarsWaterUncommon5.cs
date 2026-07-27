using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using AlchemyStars.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 小魔隐尾·贝菲尔：影镇茶话会；造成水属性伤害，消耗水光能时附加弗兰克爪击�?/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsWaterUncommon5 : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;
    private const decimal BaseDamage = 6m;
    private const int TeaPartyCooldownTurns = 2;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseDamage, ValueProp.Move),
        new PowerVar<AlchemyStarsFrankClawPower>(1m),
        AlchemyStarsKeywordText.InlineTitleVar("ShadowTownTeaParty", AlchemyStarsKeywordIds.ShadowTownTeaParty),
        AlchemyStarsKeywordText.InlineTitleVar("WaterTitle", AlchemyStarsKeywordIds.Water)
    ];

    protected override HashSet<CardTag> CanonicalTags => [AlchemyStarsCardTags.ShadowTownTeaParty];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.ShadowTownTeaParty)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.ShadowTownTeaParty)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.LightEnergy)),
        HoverTipFactory.FromPower<AlchemyStarsTeaPartyDiscountPower>(),
        HoverTipFactory.FromPower<AlchemyStarsFrankClawPower>()
    ];

    public AlchemyStarsWaterUncommon5()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await AlchemyStarsCardHelpers.TryTriggerTeaPartyOnPlay(
            choiceContext,
            this,
            Owner,
            TeaPartyCooldownTurns);

        await LightMechanic.DealElementalAttackDamage(
            choiceContext,
            Owner,
            this,
            cardPlay.Target,
            DynamicVars.Damage.BaseValue,
            LightElement.Water,
            cardPlay);

        if (LightMechanic.TryConsumeLightEnergy(Owner, [LightElement.Water]))
        {
            await PowerCmd.Apply<AlchemyStarsFrankClawPower>(
                choiceContext,
                cardPlay.Target,
                DynamicVars["AlchemyStarsFrankClawPower"].BaseValue,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["AlchemyStarsFrankClawPower"].UpgradeValueBy(1m);
    }
}
