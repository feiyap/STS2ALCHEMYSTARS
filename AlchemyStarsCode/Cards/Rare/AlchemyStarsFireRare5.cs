using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
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
/// 左轮之徒·约拿：子弹数等于消耗牌堆数量；每发伤害为基础×(1+子弹数)。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsFireRare5 : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;
    private const int RequiredFireCells = 4;
    private const decimal BaseBulletDamage = 1m;
    private const int BulletsPerExhaustCard = 1;

    protected override bool IsPlayable =>
        LightMechanic.CountFireAttributeCells(Owner) >= RequiredFireCells;

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseBulletDamage, ValueProp.Move),
        new DynamicVar("Bullets", 0),
        AlchemyStarsKeywordText.InlineTitleVar("HighNoon", AlchemyStarsKeywordIds.HighNoon),
        AlchemyStarsKeywordText.InlineTitleVar("FireTitle", AlchemyStarsKeywordIds.Fire)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.HighNoon)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.HighNoon))
    ];

    public AlchemyStarsFireRare5()
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

        SyncBulletDisplay();
        await Task.CompletedTask;
    }

    public override Task AfterCardExhausted(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool causedByEthereal)
    {
        SyncBulletDisplay();
        return Task.CompletedTask;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var bullets = GetBulletCount();
        // 1/2×(1+子弹数量)：每发 = 基础伤害 × (1+子弹数)
        var perShot = DynamicVars.Damage.BaseValue * (1 + bullets);

        for (var i = 0; i < bullets; i++)
        {
            if (cardPlay.Target.IsDead)
                break;

            await LightMechanic.DealElementalAttackDamage(
                choiceContext,
                Owner,
                this,
                cardPlay.Target,
                perShot,
                LightElement.Fire,
                cardPlay);
        }

        SyncBulletDisplay();
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
    }

    private int GetBulletCount()
    {
        if (Owner == null)
            return 0;

        return PileType.Exhaust.GetPile(Owner).Cards.Count * BulletsPerExhaustCard;
    }

    private void SyncBulletDisplay()
    {
        DynamicVars["Bullets"].BaseValue = GetBulletCount();
    }

    /// <summary>
    /// 消耗牌堆变化后，同步所有约拿牌面的子弹数。
    /// </summary>
    public static void SyncAllBulletDisplays(Player player)
    {
        var combat = player.PlayerCombatState;
        if (combat == null)
            return;

        foreach (var card in combat.AllCards)
        {
            if (card is AlchemyStarsFireRare5 jonah)
                jonah.SyncBulletDisplay();
        }
    }
}
