using System.Linq;
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
/// 凡尘之羽·安洁尔：随机多段雷伤；命中同一敌人时伤害递减；最多消耗 2 点雷光能加段。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsThunderCommon1 : ModCardTemplate
{
    private const string CalculatedHitsKey = "CalculatedHits";
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Common;
    private const TargetType CardTarget = TargetType.RandomEnemy;
    private const bool ShowInCardLibrary = true;
    private const int BaseHitCount = 4;
    private const decimal BaseHitDamage = 2m;
    private const decimal HitDamageUpgradeBy = 1m;
    private const decimal BaseMinHitDamage = 1m;
    private const decimal MinHitDamageUpgradeBy = 1m;
    private const int BonusHitCount = 1;
    private const int MaxThunderLightConsume = 2;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(BaseHitCount),
        new CalculationExtraVar(BonusHitCount),
        new CalculatedVar(CalculatedHitsKey).WithMultiplier(CountLightBonusMultiplier),
        new DamageVar(BaseHitDamage, ValueProp.Move),
        new DynamicVar("MinDamage", BaseMinHitDamage),
        AlchemyStarsKeywordText.InlineTitleVar("ThunderTitle", AlchemyStarsKeywordIds.Thunder)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder)),
        ];

    public AlchemyStarsThunderCommon1() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hitCount = DynamicVars.CalculationBase.IntValue;
        for (var n = 0; n < MaxThunderLightConsume; n++)
        {
            if (!LightMechanic.TryConsumeLightEnergy(Owner, [LightElement.Thunder]))
                break;

            hitCount += DynamicVars.CalculationExtra.IntValue;
        }

        var nextHitDamage = new Dictionary<Creature, decimal>();
        var minHitDamage = DynamicVars["MinDamage"].BaseValue;

        for (var i = 0; i < hitCount; i++)
        {
            var target = PickRandomEnemy();
            if (target == null)
                break;

            if (!nextHitDamage.TryGetValue(target, out var damage))
                damage = DynamicVars.Damage.BaseValue;

            await LightMechanic.DealElementalAttackDamage(
                choiceContext,
                Owner,
                this,
                target,
                damage,
                LightElement.Thunder,
                cardPlay);

            nextHitDamage[target] = Math.Max(minHitDamage, damage / 2m);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(HitDamageUpgradeBy);
        DynamicVars["MinDamage"].UpgradeValueBy(MinHitDamageUpgradeBy);
    }

    private Creature? PickRandomEnemy()
    {
        var enemies = CombatState?.HittableEnemies.ToList();
        if (enemies == null || enemies.Count == 0)
            return null;

        return Owner.RunState.Rng.CombatTargets.NextItem(enemies);
    }

    private static decimal CountLightBonusMultiplier(CardModel card, Creature? _)
    {
        if (card.Owner == null)
            return 0m;

        return Math.Min(MaxThunderLightConsume, LightMechanic.CountThunderLightEnergy(card.Owner));
    }
}
