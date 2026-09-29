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
/// 芒刺纹徽·普律玛：往昔溃裂；消耗森强化格获得重放，伤害随转色栏产出成长；升级后改为全体。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsForestRare1 : ModCardTemplate
{
    private const string ReplayKey = "Replay";
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;
    private const decimal BaseDamage = 5m;
    private const decimal DamageUpgradeBy = 2m;

    /// <summary>
    /// 升级后改为全体敌人；未升级时保持单体目标。
    /// </summary>
    public override TargetType TargetType =>
        IsUpgraded
            ? MegaCrit.Sts2.Core.Entities.Cards.TargetType.AllEnemies
            : MegaCrit.Sts2.Core.Entities.Cards.TargetType.AnyEnemy;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseDamage, ValueProp.Move),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar(ReplayKey).WithMultiplier(CountAvailableReplays),
        AlchemyStarsKeywordText.InlineTitleVar("PastRupture", AlchemyStarsKeywordIds.PastRupture),
        AlchemyStarsKeywordText.InlineTitleVar("ForestTitle", AlchemyStarsKeywordIds.Forest)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.PastRupture)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.PastRupture)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest))
    ];

    public AlchemyStarsForestRare1()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        if (!ReferenceEquals(card, this))
            return playCount;

        var consumedEnhanced = LightMechanic.ConsumeAllForestEnhancedCells(Owner);
        return playCount + consumedEnhanced;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        SyncPastRuptureDamageDisplay();
        var damage = DynamicVars.Damage.BaseValue;

        if (IsUpgraded)
        {
            foreach (var enemy in CombatState!.HittableEnemies.ToList())
            {
                await LightMechanic.DealElementalAttackDamage(
                    choiceContext,
                    Owner,
                    this,
                    enemy,
                    damage,
                    LightElement.Forest,
                    cardPlay);
            }

            return;
        }

        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await LightMechanic.DealElementalAttackDamage(
            choiceContext,
            Owner,
            this,
            cardPlay.Target,
            damage,
            LightElement.Forest,
            cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(DamageUpgradeBy);
        SyncPastRuptureDamageDisplay();
    }

    /// <summary>
    /// 往昔溃裂加成变化后同步牌面伤害（含升级伤害加成）。
    /// </summary>
    public void SyncPastRuptureDamageDisplay()
    {
        var upgradedBonus = IsUpgraded ? DamageUpgradeBy : 0m;
        DynamicVars.Damage.BaseValue = BaseDamage + upgradedBonus + AlchemyStarsForestState.GetPastRuptureBonus(this);
    }

    /// <summary>
    /// 当前森属性强化格数量，即打出时可获得的重放次数。
    /// </summary>
    private static decimal CountAvailableReplays(CardModel card, Creature? _)
    {
        if (card.Owner == null)
            return 0m;

        return LightMechanic.CountForestEnhancedCells(card.Owner);
    }
}
