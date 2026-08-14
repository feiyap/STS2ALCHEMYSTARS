using System.Linq;
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
/// 红油魁首·醒山：攻击前获灼燃，对目标与次要敌人造成火伤；可消耗火光能强化伤害。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsFireRare2 : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;
    private const decimal BaseDamage = 15m;
    private const decimal IgnitionGain = 10m;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseDamage, ValueProp.Move),
        AlchemyStarsKeywordText.InlineTitleVar("RedOilWrench", AlchemyStarsKeywordIds.RedOilWrench),
        AlchemyStarsKeywordText.InlineTitleVar("FireTitle", AlchemyStarsKeywordIds.Fire)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.RedOilWrench)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.RedOilWrench)),
        HoverTipFactory.FromPower<AlchemyStarsIgnitionPower>()
    ];

    public AlchemyStarsFireRare2()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await PowerCmd.Apply<AlchemyStarsIgnitionPower>(
            choiceContext,
            Owner.Creature,
            IgnitionGain,
            Owner.Creature,
            this);

        var enemies = CombatState!.HittableEnemies.ToList();
        if (enemies.Count == 0)
            return;

        var consumed = LightMechanic.TryConsumeLightEnergy(
            Owner,
            [LightElement.Fire, LightElement.Fire]);

        var baseDamage = DynamicVars.Damage.BaseValue;
        var target = cardPlay.Target;
        var allSameDistance = AlchemyStarsCardHelpers.AreEnemiesAtSameDistance(enemies);
        var targetIsNearest = AlchemyStarsCardHelpers.IsNearestEnemy(target, enemies);

        var targetDamage = baseDamage;
        if (consumed && targetIsNearest)
            targetDamage *= 2m;

        var othersDealFullDamage = consumed && allSameDistance;

        foreach (var enemy in enemies)
        {
            var damage = ReferenceEquals(enemy, target)
                ? targetDamage
                : othersDealFullDamage ? baseDamage : baseDamage / 2m;

            await LightMechanic.DealElementalAttackDamage(
                choiceContext,
                Owner,
                this,
                enemy,
                damage,
                LightElement.Fire,
                cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
    }
}
