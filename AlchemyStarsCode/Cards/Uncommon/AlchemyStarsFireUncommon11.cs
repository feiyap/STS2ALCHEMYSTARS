using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 冥河列车·卡戎：亡者庆典消耗一半卡组；向抽牌堆加入消耗牌堆数量的随机火属性牌。消耗。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsFireUncommon11 : ModCardTemplate
{
    private const int BaseEnergyCost = 0;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        AlchemyStarsKeywordText.InlineTitleVar("DeadCelebration", AlchemyStarsKeywordIds.DeadCelebration),
        AlchemyStarsKeywordText.InlineTitleVar("FireTitle", AlchemyStarsKeywordIds.Fire)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.DeadCelebration)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.DeadCelebration)),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    ];

    public AlchemyStarsFireUncommon11()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var deck = PileType.Draw.GetPile(Owner).Cards
            .Concat(PileType.Discard.GetPile(Owner).Cards)
            .ToList();
        var removeCount = deck.Count / 2;
        if (removeCount > 0)
        {
            var prioritized = deck
                .OrderByDescending(card => card.Type is CardType.Status or CardType.Curse)
                .Take(removeCount)
                .ToList();

            foreach (var card in prioritized)
                await CardCmd.Exhaust(choiceContext, card);
        }

        var exhaustCount = PileType.Exhaust.GetPile(Owner).Cards.Count;
        if (exhaustCount <= 0)
            return;

        var pool = ModelDb.CardPool<AlchemyStarsCardPool>();
        var candidates = pool
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Where(IsEligibleFireCard)
            .ToList();
        if (candidates.Count == 0)
            return;

        var created = CardFactory.GetForCombat(
            Owner,
            candidates,
            exhaustCount,
            Owner.RunState.Rng.CombatCardGeneration);

        foreach (var card in created)
        {
            if (IsUpgraded)
                CardCmd.Upgrade(card);
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Draw, Owner);
        }
    }

    private static bool IsEligibleFireCard(CardModel card) =>
        AlchemyStarsCardHelpers.HasFireKeyword(card) &&
        card.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare &&
        card is not AlchemyStarsFireUncommon11 &&
        card.Type is not CardType.Status and not CardType.Curse;

    protected override void OnUpgrade()
    {
    }
}
