using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 寂静猎兵·镇魂座：对所有敌人造成雷属性伤害，力量增幅翻倍，可消耗威能追加攻击次数。
/// </summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class AlchemyStarsOldSealRequiem : ModCardTemplate, IAlchemyStarsOldSeal
{
    private const string CalculatedHitsKey = "CalculatedHits";
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Ancient;
    private const TargetType CardTarget = TargetType.AllEnemies;
    private const bool ShowInCardLibrary = true;
    private const decimal HitDamage = 2m;

    public override bool CanBeGeneratedInCombat => false;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(HitDamage, ValueProp.Move),
        new RepeatVar(1),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar(CalculatedHitsKey).WithMultiplier(CountHits),
        AlchemyStarsKeywordText.InlineTitleVar("SilentHunter", AlchemyStarsKeywordIds.SilentHunter),
        AlchemyStarsKeywordText.InlineTitleVar("RequiemRoar", AlchemyStarsKeywordIds.RequiemRoar),
        AlchemyStarsKeywordText.InlineTitleVar("ThunderTitle", AlchemyStarsKeywordIds.Thunder)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.SilentHunter),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.RequiemRoar)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.SilentHunter)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.RequiemRoar)),
        HoverTipFactory.FromPower<Powers.AlchemyStarsOldSealMightPower>(),
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    public AlchemyStarsOldSealRequiem()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    public override Task AfterCardDrawn(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw) =>
        AlchemyStarsOldSeal.GainHunterMightIfEnteredHand(choiceContext, this, card);

    public override Task AfterCardGeneratedForCombat(CardModel card, Player? creator) =>
        AlchemyStarsOldSeal.GainHunterMightIfEnteredHand(new BlockingPlayerChoiceContext(), this, card);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hits = DynamicVars.Repeat.IntValue;
        hits += await AlchemyStarsOldSeal.ConsumeAllMight(choiceContext, Owner);

        await AlchemyStarsOldSeal.DealToAll(
            choiceContext,
            Owner,
            this,
            cardPlay,
            DynamicVars.Damage.BaseValue,
            LightElement.Thunder,
            hits);
    }

    /// <summary>
    /// 镇魂怒号：力量能力本身会加上层数，这里再补同样一份，增幅变成 2 倍。
    /// 只加层数，不把整段伤害再乘 2。
    /// </summary>
    public override decimal ModifyDamageAdditive(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (!ReferenceEquals(cardSource, this) || dealer == null || Owner == null || dealer.Player != Owner || !props.IsPoweredAttack())
            return 0m;

        return dealer.GetPower<StrengthPower>()?.Amount ?? 0m;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1m);
    }

    private static decimal CountHits(CardModel card, Creature? _)
    {
        if (card.Owner == null)
            return card.DynamicVars.Repeat.BaseValue;

        return card.DynamicVars.Repeat.BaseValue + AlchemyStarsOldSeal.GetMight(card.Owner);
    }
}
