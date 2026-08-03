using System.Linq;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace AlchemyStars.Cards;

/// <summary>
/// 半生黯星·伊芙：多段雷伤；击杀后换目标；可消耗雷光能永久增加攻击次数。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsThunderRare1 : ModCardTemplate
{
    private static readonly AttachedState<CardModel, int> BonusHitCount = new(_ => 0);

    private const string CalculatedHitsKey = "CalculatedHits";
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;
    private const int BaseHitCount = 6;
    private const int BonusHitPerUse = 4;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(BaseHitCount),
        new CalculationExtraVar(1m),
        new CalculatedVar(CalculatedHitsKey).WithMultiplier(CountCurrentHits),
        new DamageVar(1m, ValueProp.Move),
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

    public AlchemyStarsThunderRare1()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LightMechanic.TryConsumeLightEnergy(Owner, [LightElement.Thunder]))
            BonusHitCount[this] += BonusHitPerUse;

        // 实际次数只取基础+已累计；预览里的「即将消耗光能+4」不能在此再算一次。
        var hitCount = DynamicVars.CalculationBase.IntValue + BonusHitCount[this];
        var damage = DynamicVars.Damage.BaseValue;
        var target = cardPlay.Target;

        while (true)
        {
            if (target == null || target.IsDead)
            {
                target = PickRandomEnemy();
                if (target == null)
                    break;
            }

            for (var i = 0; i < hitCount; i++)
            {
                if (target.IsDead)
                    break;

                await LightMechanic.DealElementalAttackDamage(
                    choiceContext,
                    Owner,
                    this,
                    target,
                    damage,
                    LightElement.Thunder,
                    cardPlay);
            }

            if (!target.IsDead)
                break;

            target = null;
        }
    }

    private Creature? PickRandomEnemy()
    {
        var enemies = CombatState?.HittableEnemies.ToList();
        if (enemies == null || enemies.Count == 0)
            return null;

        return Owner.RunState.Rng.CombatTargets.NextItem(enemies);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
    }

    /// <summary>
    /// 已累计的额外次数；若当前可消耗雷光能，再计入即将获得的 +4。
    /// </summary>
    private static decimal CountCurrentHits(CardModel card, Creature? _)
    {
        var bonus = BonusHitCount[card];
        if (card.Owner != null && LightMechanic.HasThunderLightEnergy(card.Owner))
            bonus += BonusHitPerUse;

        return bonus;
    }
}
