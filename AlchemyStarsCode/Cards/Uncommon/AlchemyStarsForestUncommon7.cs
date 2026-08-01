using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
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
using STS2RitsuLib.Utils;

namespace AlchemyStars.Cards;

/// <summary>
/// 结晶血域·拜里厄：手牌中时未格挡森攻击施加结晶；打出时引爆结晶并造成群体森伤�?/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsForestUncommon7 : ModCardTemplate
{
    private static readonly AttachedState<CardModel, bool> GrantedDomainPower = new(_ => false);

    private const int BaseEnergyCost = 3;
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
        HoverTipFactory.FromPower<AlchemyStarsCrystallizationDomainPower>(),
        HoverTipFactory.FromPower<AlchemyStarsCrystallizationPower>(),
        HoverTipFactory.FromKeyword(CardKeyword.Retain)
    ];

    public AlchemyStarsForestUncommon7()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    public override async Task AfterCardDrawn(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw)
    {
        if (!ReferenceEquals(card, this))
            return;

        await EnsureCrystallizationDomainPower(choiceContext);
    }

    public override async Task AfterFlush(
        PlayerChoiceContext choiceContext,
        Player player,
        IReadOnlyCollection<CardModel> flushedCards,
        IReadOnlyCollection<CardModel> retainedCards)
    {
        if (player != Owner)
            return;

        if (retainedCards.Contains(this))
            await EnsureCrystallizationDomainPower(choiceContext);

        if (flushedCards.Contains(this))
            await RemoveCrystallizationDomainPowerIfGranted(choiceContext);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await RemoveCrystallizationDomainPowerIfGranted(choiceContext);

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

    private async Task EnsureCrystallizationDomainPower(PlayerChoiceContext choiceContext)
    {
        if (GrantedDomainPower[this])
            return;

        if (Owner.Creature.GetPowerAmount<AlchemyStarsCrystallizationDomainPower>() > 0)
        {
            GrantedDomainPower[this] = true;
            return;
        }

        await PowerCmd.Apply<AlchemyStarsCrystallizationDomainPower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            this);
        GrantedDomainPower[this] = true;
    }

    private async Task RemoveCrystallizationDomainPowerIfGranted(PlayerChoiceContext choiceContext)
    {
        if (!GrantedDomainPower[this])
            return;

        var power = Owner.Creature.GetPower<AlchemyStarsCrystallizationDomainPower>();
        if (power != null)
            await PowerCmd.Remove(power);

        GrantedDomainPower[this] = false;
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
