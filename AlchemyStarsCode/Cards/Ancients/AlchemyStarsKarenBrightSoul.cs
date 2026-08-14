using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 卡莲·煜魂：先古技能。高庭卫队；按火/水光能造成火伤并获得等额格挡。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsKarenBrightSoul : ModCardTemplate
{
    private const string CalculatedHitsKey = "CalculatedHits";
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Ancient;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;
    private const decimal HitDamage = 6m;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1),
        new DamageVar(HitDamage, ValueProp.Move),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar(CalculatedHitsKey).WithMultiplier(CountFireAndWaterLightHits),
        AlchemyStarsKeywordText.InlineTitleVar("HighCourtGuard", AlchemyStarsKeywordIds.HighCourtGuard),
        AlchemyStarsKeywordText.InlineTitleVar("FireTitle", AlchemyStarsKeywordIds.Fire),
        AlchemyStarsKeywordText.InlineTitleVar("WaterTitle", AlchemyStarsKeywordIds.Water)
    ];

    protected override HashSet<CardTag> CanonicalTags => [AlchemyStarsCardTags.HighCourtGuard];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.HighCourtGuard)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.HighCourtGuard))
    ];

    public AlchemyStarsKarenBrightSoul()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    public override bool TryModifyEnergyCostInCombatLate(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (!ReferenceEquals(card, this))
            return false;

        if (!AlchemyStarsCardHelpers.HasOtherTagInHand(this, Owner, AlchemyStarsCardTags.HighCourtGuard))
            return false;

        modifiedCost = 0m;
        return modifiedCost != originalCost;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (AlchemyStarsCardHelpers.HasOtherTagInHand(this, Owner, AlchemyStarsCardTags.HighCourtGuard))
            await PlayerCmd.GainEnergy(1, Owner);

        LightMechanic.TryGrantLightEnergy(Owner, LightElement.Fire);
        LightMechanic.TryGrantLightEnergy(Owner, LightElement.Water);

        var target = cardPlay.Target;
        if (target == null || target.IsDead)
            return;

        var hitCount = LightMechanic.CountFireAndWaterLightEnergy(Owner);
        var hitDamage = DynamicVars.Damage.BaseValue;

        for (var i = 0; i < hitCount; i++)
        {
            if (target.IsDead)
                break;

            decimal totalDamage;
            using (LightMechanicDamageContext.Use(LightElement.Fire))
            {
                var attack = DamageCmd.Attack(hitDamage)
                    .FromCard(this, cardPlay)
                    .Targeting(target);
                await attack.Execute(choiceContext);
                totalDamage = attack.Results
                    .SelectMany(result => result)
                    .Sum(result => (decimal)result.TotalDamage);
            }

            await LightMechanic.ApplyElementalHitEffects(
                choiceContext,
                Owner,
                target,
                LightElement.Fire,
                this);

            if (totalDamage > 0m)
                await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(totalDamage, ValueProp.Move), cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    /// <summary>
    /// 当前火/水光能数量（含万色）；预览包含打出时将获得的 1 火 + 1 水。
    /// </summary>
    private static decimal CountFireAndWaterLightHits(CardModel card, Creature? _)
    {
        if (card.Owner == null)
            return 0m;

        // 打出时会先获得 1 火 + 1 水光能，预览一并计入。
        return LightMechanic.CountFireAndWaterLightEnergy(card.Owner) + 2;
    }
}
