using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 心之怪盗团：万色攻击；牌自身监听破盾/受击以触发追击与总攻击；大罪穿甲弹无视防御并斩杀偷金。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsRare3 : ModCardTemplate
{
    private const int BaseEnergyCost = 7;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;
    private const int BaseDamageMultiplier = 4;
    private const int UpgradedDamageMultiplier = 6;
    private const decimal ExecuteHpPercent = 0.1m;
    private const int ExecuteGold = 25;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("DamageMult", BaseDamageMultiplier),
        AlchemyStarsKeywordText.InlineTitleVar("Pursuit", AlchemyStarsKeywordIds.Pursuit),
        AlchemyStarsKeywordText.InlineTitleVar("AllOutAttack", AlchemyStarsKeywordIds.AllOutAttack),
        AlchemyStarsKeywordText.InlineTitleVar("SinPiercingRound", AlchemyStarsKeywordIds.SinPiercingRound),
        AlchemyStarsKeywordText.InlineTitleVar("PrismaticTitle", AlchemyStarsKeywordIds.Prismatic)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Prismatic),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Pursuit),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.AllOutAttack),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.SinPiercingRound)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Prismatic)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Pursuit)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.AllOutAttack)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.SinPiercingRound)),
        HoverTipFactory.FromPower<HardToKillPower>(),
        HoverTipFactory.FromPower<BufferPower>(),
        HoverTipFactory.FromPower<SlipperyPower>(),
        HoverTipFactory.FromPower<IntangiblePower>(),
        HoverTipFactory.Static(StaticHoverTip.Fatal)
    ];

    public AlchemyStarsRare3()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    /// <summary>
    /// 战斗堆中实时监听破盾/受击，触发追击与总攻击（无需额外能力图标）。
    /// 多副本时仅由第一张本卡响应，避免重复触发。
    /// </summary>
    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (!IsPrimaryWatchInstance())
            return;

        await AlchemyStarsPhantomThievesTracker.AfterDamageReceived(
            choiceContext,
            target,
            result,
            props,
            dealer,
            cardSource);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var target = cardPlay.Target;

        await AlchemyStarsCardHelpers.ClearPenetratingDefenses(choiceContext, target);
        if (target.HasPower<IntangiblePower>())
            await PowerCmd.Remove<IntangiblePower>(target);

        var teammates = CombatState?.GetTeammatesOf(Owner.Creature).Count() ?? 0;
        var attributeCells = LightMechanic.CountAttributeCells(Owner);
        var mult = DynamicVars["DamageMult"].IntValue;
        var damage = (teammates + attributeCells) * mult;

        await LightMechanic.DealElementalAttackDamage(
            choiceContext,
            Owner,
            this,
            target,
            damage,
            LightElement.Prismatic,
            cardPlay);

        // CurrentHp/MaxHp 为 int，不可直接相除；与弥加德等共用阈值判定。
        await AlchemyStarsCardHelpers.TryExecuteBelowHpThreshold(
            choiceContext,
            target,
            ExecuteHpPercent);

        if (target.IsDead)
            await PlayerCmd.GainGold(ExecuteGold, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["DamageMult"].UpgradeValueBy(UpgradedDamageMultiplier - BaseDamageMultiplier);
    }

    /// <summary>
    /// 追击/总攻击：尝试自动打出本卡。
    /// </summary>
    public static async Task TryAutoPlay(
        PlayerChoiceContext choiceContext,
        Player owner,
        Creature target,
        bool requireInHand)
    {
        if (CombatManager.Instance.IsOverOrEnding)
            return;

        var card = FindInCombatPiles(owner, requireInHand);
        if (card == null || target.IsDead)
            return;

        if (card.Pile?.Type != PileType.Hand)
            await CardPileCmd.Add(card, PileType.Hand);

        await CardCmd.AutoPlay(choiceContext, card, target);
    }

    /// <summary>
    /// 战斗堆中第一张本卡负责监听，避免多副本重复触发。
    /// </summary>
    private bool IsPrimaryWatchInstance()
    {
        var primary = FindInCombatPiles(Owner, requireInHand: false);
        return primary != null && ReferenceEquals(primary, this);
    }

    private static AlchemyStarsRare3? FindInCombatPiles(Player owner, bool requireInHand)
    {
        var piles = requireInHand
            ? new[] { PileType.Hand }
            : new[] { PileType.Hand, PileType.Draw, PileType.Discard, PileType.Exhaust };

        foreach (var pileType in piles)
        {
            var card = pileType.GetPile(owner).Cards.OfType<AlchemyStarsRare3>().FirstOrDefault();
            if (card != null)
                return card;
        }

        return null;
    }
}
