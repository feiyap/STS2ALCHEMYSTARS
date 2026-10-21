using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 寂静猎兵·焚霜座：打出时消耗全部状态与诅咒并获得威能，再按威能与消耗牌堆造成火属性伤害。
/// </summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class AlchemyStarsOldSealFrost : AlchemyStarsOldSealAttackCard
{
    private const string CalculatedDamageKey = "CalculatedDamage";
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Ancient;
    private const TargetType CardTarget = TargetType.AllEnemies;
    private const bool ShowInCardLibrary = true;

    public override bool CanBeGeneratedInCombat => false;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(1m, ValueProp.Move),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar(CalculatedDamageKey).WithMultiplier(CountDamage),
        AlchemyStarsKeywordText.InlineTitleVar("SilentHunter", AlchemyStarsKeywordIds.SilentHunter),
        AlchemyStarsKeywordText.InlineTitleVar("FrostHeart", AlchemyStarsKeywordIds.FrostHeart),
        AlchemyStarsKeywordText.InlineTitleVar("FireTitle", AlchemyStarsKeywordIds.Fire)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.SilentHunter),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.FrostHeart)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.SilentHunter)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.FrostHeart)),
        HoverTipFactory.FromPower<Powers.AlchemyStarsOldSealMightPower>()
    ];

    public AlchemyStarsOldSealFrost()
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
        var targets = new[] { PileType.Hand, PileType.Draw, PileType.Discard };
        var cursesAndStatuses = targets
            .SelectMany(pile => pile.GetPile(Owner).Cards)
            .Where(card => card.Type is CardType.Status or CardType.Curse)
            .ToList();

        foreach (var card in cursesAndStatuses)
            await CardCmd.Exhaust(choiceContext, card);

        if (cursesAndStatuses.Count > 0)
        {
            await AlchemyStarsOldSeal.GainMight(
                choiceContext,
                Owner,
                this,
                cursesAndStatuses.Count);
        }

        var exhaustCount = PileType.Exhaust.GetPile(Owner).Cards.Count;
        await DealScaledToAll(choiceContext, cardPlay, exhaustCount, LightElement.Fire);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
    }

    private static decimal CountDamage(CardModel card, Creature? _)
    {
        if (card.Owner == null)
            return 0m;

        var exhaustCount = PileType.Exhaust.GetPile(card.Owner).Cards.Count;
        return AlchemyStarsOldSeal.ScaleDamage(card, _, exhaustCount, null);
    }
}
