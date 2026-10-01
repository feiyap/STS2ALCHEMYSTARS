using System.Linq;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using AlchemyStars.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 勇士之锋·克：仅可打攻击意图敌人；承受半额即时/半额回合末；倍火返还并吸血。
/// </summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class AlchemyStarsKe : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = false;
    private const decimal BaseReturnMultiplier = 2m;
    private const decimal UpgradedReturnMultiplier = 3m;
    private const decimal HealRatio = 0.15m;

    public override bool CanBeGeneratedInCombat => false;

    public override bool CanBeGeneratedByModifiers => false;

    protected override bool IsPlayable =>
        CombatState?.HittableEnemies.Any(e => e.Monster?.IntendsToAttack == true) == true;

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("ReturnMult", BaseReturnMultiplier),
        new DynamicVar("HealPercent", 15m),
        AlchemyStarsKeywordText.InlineTitleVar("FireTitle", AlchemyStarsKeywordIds.Fire)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.BraveTigerRoar),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Endure)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.BraveTigerRoar)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Endure)),
        HoverTipFactory.FromPower<AlchemyStarsEndureBacklashPower>()
    ];

    public AlchemyStarsKe()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var target = cardPlay.Target;
        if (target.Monster?.IntendsToAttack != true)
            return;

        var total = ResolveIntendedAttackDamage(target);
        if (total <= 0m)
            return;

        var immediate = Math.Ceiling(total / 2m);
        var deferred = total - immediate;

        // 敌人为 dealer，承受伤害可被格挡。
        await CreatureCmd.Damage(
            choiceContext,
            Owner.Creature,
            immediate,
            ValueProp.Move,
            target);

        if (deferred > 0m)
        {
            await PowerCmd.Apply<AlchemyStarsEndureBacklashPower>(
                choiceContext,
                Owner.Creature,
                deferred,
                Owner.Creature,
                this);
        }

        var returnMult = IsUpgraded ? UpgradedReturnMultiplier : BaseReturnMultiplier;
        var returnDamage = total * returnMult;

        decimal dealtUnblocked;
        using (LightMechanicDamageContext.Use(LightElement.Fire))
        {
            var attack = DamageCmd.Attack(returnDamage)
                .FromCard(this, cardPlay)
                .Targeting(target);
            await attack.Execute(choiceContext);
            dealtUnblocked = attack.Results
                .SelectMany(result => result)
                .Sum(result => (decimal)result.UnblockedDamage);
        }

        await LightMechanic.ApplyElementalHitEffects(
            choiceContext,
            Owner,
            target,
            LightElement.Fire,
            this);

        var heal = Math.Floor(dealtUnblocked * HealRatio);
        if (heal > 0m)
            await CreatureCmd.Heal(Owner.Creature, heal);
    }

    private decimal ResolveIntendedAttackDamage(Creature enemy)
    {
        var monster = enemy.Monster;
        if (monster == null)
            return 0m;

        var targets = new[] { Owner.Creature };
        decimal total = 0m;
        foreach (var intent in monster.NextMove.Intents)
        {
            if (intent is AttackIntent attack)
                total += attack.GetTotalDamage(targets, enemy);
        }

        return total;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ReturnMult"].UpgradeValueBy(1m);
    }
}
