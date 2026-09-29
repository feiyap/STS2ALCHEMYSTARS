using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
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
/// 心之怪盗团：万色攻击；追击/总攻击自动打出；大罪穿甲弹无视防御并斩杀偷金。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsRare3 : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
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

        if (!target.IsDead && target.MaxHp > 0m && target.CurrentHp / target.MaxHp < ExecuteHpPercent)
            await CreatureCmd.Kill(target);

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

        var piles = requireInHand
            ? new[] { PileType.Hand }
            : new[] { PileType.Hand, PileType.Draw, PileType.Discard, PileType.Exhaust };

        AlchemyStarsRare3? card = null;
        foreach (var pileType in piles)
        {
            card = pileType.GetPile(owner).Cards.OfType<AlchemyStarsRare3>().FirstOrDefault();
            if (card != null)
                break;
        }

        if (card == null || target.IsDead)
            return;

        if (card.Pile?.Type != PileType.Hand)
            await CardPileCmd.Add(card, PileType.Hand);

        await CardCmd.AutoPlay(choiceContext, card, target);
    }
}
