using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using AlchemyStars.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 结晶血域·拜里厄：手牌中时结晶领域生效；打出时消耗森光能造成群体森伤并引爆结晶。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsForestUncommon7 : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.AllEnemies;
    private const bool ShowInCardLibrary = true;
    private const int RequiredForestLightEnergy = 2;
    private const decimal DetonatePercentPerStack = 0.02m;
    private const int SingleTargetMultiplier = 4;

    protected override bool IsPlayable =>
        LightMechanic.HasForestLightEnergyCount(Owner, RequiredForestLightEnergy);

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5m, ValueProp.Move),
        AlchemyStarsKeywordText.InlineTitleVar("CrystallizationDomain", AlchemyStarsKeywordIds.CrystallizationDomain),
        AlchemyStarsKeywordText.InlineTitleVar("ForestTitle", AlchemyStarsKeywordIds.Forest)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.CrystallizationDomain),
        CardKeyword.Retain
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.CrystallizationDomain)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Crystallization)),
        HoverTipFactory.FromKeyword(CardKeyword.Retain)
    ];

    public AlchemyStarsForestUncommon7()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    /// <summary>
    /// 手牌中时：未被格挡的森属性攻击命中，为目标施加 1 层结晶。
    /// </summary>
    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (Pile?.Type != PileType.Hand)
            return;

        if (dealer != Owner.Creature || target.IsDead || target.Side == Owner.Creature.Side)
            return;

        if (!props.IsPoweredAttack() || result.UnblockedDamage <= 0)
            return;

        if (LightMechanicDamageContext.CurrentElement != LightElement.Forest)
            return;

        await PowerCmd.Apply<AlchemyStarsCrystallizationPower>(
            choiceContext,
            target,
            1m,
            Owner.Creature,
            this);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        LightMechanic.TryConsumeLightEnergy(
            Owner,
            [LightElement.Forest, LightElement.Forest]);

        var enemies = CombatState!.HittableEnemies.ToList();
        if (enemies.Count == 0)
            return;

        var baseDamage = DynamicVars.Damage.BaseValue * enemies.Count;
        if (enemies.Count == 1)
            baseDamage *= SingleTargetMultiplier;

        foreach (var enemy in enemies)
        {
            await LightMechanic.DealElementalAttackDamage(
                choiceContext,
                Owner,
                this,
                enemy,
                baseDamage,
                LightElement.Forest,
                cardPlay);
        }

        foreach (var enemy in enemies.ToList())
            await DetonateCrystallization(choiceContext, enemy);
    }

    private static async Task DetonateCrystallization(PlayerChoiceContext choiceContext, Creature enemy)
    {
        var crystallization = enemy.GetPower<AlchemyStarsCrystallizationPower>();
        if (crystallization == null || crystallization.Amount <= 0 || enemy.IsDead)
            return;

        var loss = enemy.MaxHp * DetonatePercentPerStack * crystallization.Amount;
        if (loss <= 0m)
            return;

        await CreatureCmd.Damage(
            choiceContext,
            enemy,
            loss,
            ValueProp.Unblockable | ValueProp.Unpowered,
            null,
            null);

        await PowerCmd.Remove(crystallization);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
