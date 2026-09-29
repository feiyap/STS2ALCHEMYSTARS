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
/// 寂默之花·库斯库塔：影镇茶话会；保留时造成随机森伤并复制自身，打出时提升全场库斯库塔伤害。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsForestUncommon8 : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.RandomEnemy;
    private const bool ShowInCardLibrary = true;
    private const int RetainBaseDamage = 5;
    private const int RetainDamageUpgradeBy = 1;
    private const int PlayBonusBase = 1;
    private const int PlayBonusUpgradeBy = 1;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(RetainBaseDamage, ValueProp.Move),
        new IntVar("Increase", PlayBonusBase),
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

        // 保留：只造成伤害并复制，不提升伤害。
        var damage = GetCombatDamage();
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

        var copy = CombatState!.CreateCard<AlchemyStarsForestUncommon8>(Owner);
        if (IsUpgraded)
            copy.UpgradeInternal();
        copy.SyncDamageDisplay();

        await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Discard, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(RetainDamageUpgradeBy);
        DynamicVars["Increase"].UpgradeValueBy(PlayBonusUpgradeBy);
        SyncDamageDisplay();
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await AlchemyStarsCardHelpers.TryTriggerTeaPartyOnPlay(choiceContext, this, Owner);

        var damage = GetCombatDamage();
        var target = PickRandomEnemy();
        if (target != null)
        {
            await LightMechanic.DealElementalAttackDamage(
                choiceContext,
                Owner,
                this,
                target,
                damage,
                LightElement.Forest,
                cardPlay);
        }

        // 打出：本场战斗所有库斯库塔伤害提升。
        AlchemyStarsForestState.IncrementKushkutaCombatDamageBonus(
            Owner,
            DynamicVars["Increase"].IntValue);
        SyncAllKushkutaDamageDisplays(Owner);
    }

    private decimal GetFaceDamage() =>
        IsUpgraded ? RetainBaseDamage + RetainDamageUpgradeBy : RetainBaseDamage;

    private decimal GetCombatDamage() =>
        GetFaceDamage() + AlchemyStarsForestState.GetKushkutaCombatDamageBonus(Owner);

    private void SyncDamageDisplay()
    {
        DynamicVars.Damage.BaseValue = GetCombatDamage();
    }

    private static void SyncAllKushkutaDamageDisplays(Player player)
    {
        var combat = player.PlayerCombatState;
        if (combat == null)
            return;

        foreach (var card in combat.AllCards)
        {
            if (card is AlchemyStarsForestUncommon8 kushkuta)
                kushkuta.SyncDamageDisplay();
        }
    }

    private Creature? PickRandomEnemy()
    {
        var enemies = CombatState?.HittableEnemies.ToList();
        if (enemies == null || enemies.Count == 0)
            return null;

        return Owner.RunState.Rng.CombatTargets.NextItem(enemies);
    }
}
