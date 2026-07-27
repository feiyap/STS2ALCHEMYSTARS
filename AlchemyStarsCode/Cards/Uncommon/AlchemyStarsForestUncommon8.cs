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
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 寂默之花·库斯库塔：影镇茶话会；保留时造成随机森伤并复制自身，打出时消耗并随机攻击�?/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsForestUncommon8 : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.RandomEnemy;
    private const bool ShowInCardLibrary = true;
    private const int RetainBaseDamage = 5;
    private const int RetainBonusBase = 1;
    private const int RetainBonusUpgradeBy = 1;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(RetainBaseDamage, ValueProp.Move),
        new IntVar("Increase", RetainBonusBase),
        AlchemyStarsKeywordText.InlineTitleVar("ShadowTownTeaParty", AlchemyStarsKeywordIds.ShadowTownTeaParty),
        AlchemyStarsKeywordText.InlineTitleVar("ForestTitle", AlchemyStarsKeywordIds.Forest)
    ];

    protected override HashSet<CardTag> CanonicalTags => [AlchemyStarsCardTags.ShadowTownTeaParty];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.ShadowTownTeaParty),
        CardKeyword.Retain,
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.ShadowTownTeaParty)),
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    ];

    public AlchemyStarsForestUncommon8()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    public override async Task AfterFlush(
        PlayerChoiceContext choiceContext,
        Player player,
        IReadOnlyCollection<CardModel> flushedCards,
        IReadOnlyCollection<CardModel> retainedCards)
    {
        if (player != Owner || !retainedCards.Contains(this))
            return;

        AlchemyStarsForestState.IncrementRetainEffectCount(player);

        var bonus = AlchemyStarsForestState.GetKushkutaCombatDamageBonus(this);
        var damage = DynamicVars.Damage.IntValue + bonus;
        var target = PickRandomEnemy();
        if (target != null)
        {
            await LightMechanic.DealElementalAttackDamage(
                choiceContext,
                Owner,
                this,
                target,
                damage,
                LightElement.Forest);
        }

        AlchemyStarsForestState.IncrementKushkutaCombatDamageBonus(
            this,
            DynamicVars["Increase"].IntValue);

        var copy = CombatState!.CreateCard<AlchemyStarsForestUncommon8>(Owner);
        if (IsUpgraded)
            copy.UpgradeInternal();

        await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Discard, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Increase"].UpgradeValueBy(RetainBonusUpgradeBy);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var bonus = AlchemyStarsForestState.GetKushkutaCombatDamageBonus(this);
        var damage = DynamicVars.Damage.IntValue + bonus;
        var target = PickRandomEnemy();
        if (target == null)
            return;

        await LightMechanic.DealElementalAttackDamage(
            choiceContext,
            Owner,
            this,
            target,
            damage,
            LightElement.Forest,
            cardPlay);
    }

    private Creature? PickRandomEnemy()
    {
        var enemies = CombatState?.HittableEnemies.ToList();
        if (enemies == null || enemies.Count == 0)
            return null;

        return Owner.RunState.Rng.CombatTargets.NextItem(enemies);
    }
}
