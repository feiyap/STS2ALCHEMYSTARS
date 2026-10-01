using System.Linq;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using AlchemyStars.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 蓝颜祸水·弗劳尔：随机水伤；低血增伤；机工印记转移/双击。
/// </summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class AlchemyStarsFlower : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.RandomEnemy;
    private const bool ShowInCardLibrary = false;
    private const decimal BaseDamage = 9m;
    private const decimal DamageUpgradeBy = 3m;
    private const decimal LowHpBonus = 1.4m;

    public override bool CanBeGeneratedInCombat => false;

    public override bool CanBeGeneratedByModifiers => false;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseDamage, ValueProp.Move),
        AlchemyStarsKeywordText.InlineTitleVar("WaterTitle", AlchemyStarsKeywordIds.Water),
        AlchemyStarsKeywordText.InlineTitleVar("MechMark", AlchemyStarsKeywordIds.MechMark)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.MechMaster),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.MechMark)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.MechMaster)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.MechMark)),
        HoverTipFactory.FromPower<AlchemyStarsMechMarkPower>()
    ];

    public AlchemyStarsFlower()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = PickRandomEnemy();
        if (target == null)
            return;

        var damage = DynamicVars.Damage.BaseValue;
        if (target.MaxHp > 0m && target.CurrentHp / target.MaxHp < 0.5m)
            damage *= LowHpBonus;

        var hadMark = target.GetPower<AlchemyStarsMechMarkPower>() != null;

        await LightMechanic.DealElementalAttackDamage(
            choiceContext,
            Owner,
            this,
            target,
            damage,
            LightElement.Water,
            cardPlay);

        if (hadMark && !target.IsDead)
        {
            await LightMechanic.DealElementalAttackDamage(
                choiceContext,
                Owner,
                this,
                target,
                damage,
                LightElement.Water,
                cardPlay,
                playAttackerAnim: false);
        }

        await TransferMechMark(choiceContext, target);
    }

    private async Task TransferMechMark(PlayerChoiceContext choiceContext, Creature newTarget)
    {
        if (CombatState == null)
            return;

        foreach (var enemy in CombatState.GetCreaturesOnSide(CombatSide.Enemy).ToList())
        {
            var existing = enemy.GetPower<AlchemyStarsMechMarkPower>();
            if (existing != null)
                await PowerCmd.Remove(existing);
        }

        if (!newTarget.IsDead)
        {
            await PowerCmd.Apply<AlchemyStarsMechMarkPower>(
                choiceContext,
                newTarget,
                1m,
                Owner.Creature,
                this);
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
        DynamicVars.Damage.UpgradeValueBy(DamageUpgradeBy);
    }
}
