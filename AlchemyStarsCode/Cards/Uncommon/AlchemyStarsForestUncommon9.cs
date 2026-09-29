using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 铁棘冠冕·列奥：荆印在身；耗森光对敌造成单段森伤，伤害随本场保留效果次数成长。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsForestUncommon9 : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;
    private const decimal BaseHitDamage = 9m;
    private const decimal DamageUpgradeBy = 5m;

    protected override bool IsPlayable => LightMechanic.HasForestLightEnergy(Owner);

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseHitDamage, ValueProp.Move),
        AlchemyStarsKeywordText.InlineTitleVar("ThornSealOnBody", AlchemyStarsKeywordIds.ThornSealOnBody),
        AlchemyStarsKeywordText.InlineTitleVar("ForestTitle", AlchemyStarsKeywordIds.Forest)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.ThornSealOnBody)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.ThornSealOnBody))
    ];

    public AlchemyStarsForestUncommon9()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        if (!LightMechanic.TryConsumeLightEnergy(Owner, [LightElement.Forest]))
            return;

        SyncDamageDisplay();
        await LightMechanic.DealElementalAttackDamage(
            choiceContext,
            Owner,
            this,
            cardPlay.Target,
            DynamicVars.Damage.BaseValue,
            LightElement.Forest,
            cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(DamageUpgradeBy);
        if (Owner?.PlayerCombatState != null)
            SyncDamageDisplay();
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        ResetDamageDisplay();
        return Task.CompletedTask;
    }

    private decimal GetBaseHitDamage() =>
        IsUpgraded ? BaseHitDamage + DamageUpgradeBy : BaseHitDamage;

    private decimal GetCombatDamage()
    {
        var baseDamage = GetBaseHitDamage();
        if (Owner?.PlayerCombatState == null)
            return baseDamage;

        return baseDamage + AlchemyStarsForestState.GetRetainEffectCount(Owner);
    }

    private void SyncDamageDisplay()
    {
        DynamicVars.Damage.BaseValue = GetCombatDamage();
    }

    private void ResetDamageDisplay()
    {
        DynamicVars.Damage.BaseValue = GetBaseHitDamage();
    }

    /// <summary>
    /// 保留效果计数变化后，同步所有列奥牌面伤害数字。
    /// </summary>
    public static void SyncAllThornSealDamageDisplays(Player player)
    {
        var combat = player.PlayerCombatState;
        if (combat == null)
            return;

        foreach (var card in combat.AllCards)
        {
            if (card is AlchemyStarsForestUncommon9 leo)
                leo.SyncDamageDisplay();
        }
    }

    /// <summary>
    /// 战斗结束后恢复列奥牌面为基础伤害。
    /// </summary>
    public static void ResetAllThornSealDamageDisplays(Player player)
    {
        foreach (var card in player.Deck.Cards)
        {
            if (card is AlchemyStarsForestUncommon9 leo)
                leo.ResetDamageDisplay();
        }

        var combat = player.PlayerCombatState;
        if (combat == null)
            return;

        foreach (var card in combat.AllCards)
        {
            if (card is AlchemyStarsForestUncommon9 leo)
                leo.ResetDamageDisplay();
        }
    }
}
