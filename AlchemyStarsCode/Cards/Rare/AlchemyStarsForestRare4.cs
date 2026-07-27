using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 控牌魔手·杰诺：将起源卡牌置入手牌；每次被保留时本场战斗费用减 1。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsForestRare4 : ModCardTemplate
{
    private const int BaseEnergyCost = 3;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest)),
        HoverTipFactory.FromCard<AlchemyStarsGeneratedOriginSun>(),
        HoverTipFactory.FromCard<AlchemyStarsGeneratedOriginMoon>(),
        HoverTipFactory.FromCard<AlchemyStarsGeneratedOriginStar>()
    ];

    public AlchemyStarsForestRare4()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    public override bool TryModifyEnergyCostInCombat(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (!ReferenceEquals(card, this))
            return false;

        var retainCount = AlchemyStarsForestState.GetJenoRetainCount(this);
        if (retainCount <= 0)
            return false;

        modifiedCost = Math.Max(0m, originalCost - retainCount);
        return modifiedCost != originalCost;
    }

    public override async Task AfterFlush(
        PlayerChoiceContext choiceContext,
        Player player,
        IReadOnlyCollection<CardModel> flushedCards,
        IReadOnlyCollection<CardModel> retainedCards)
    {
        if (player != Owner || retainedCards.All(card => !ReferenceEquals(card, this)))
            return;

        AlchemyStarsForestState.IncrementJenoRetainCount(this);
        AlchemyStarsForestState.IncrementRetainEffectCount(player);
        InvokeEnergyCostChanged();
        await Task.CompletedTask;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await AddOriginCard<AlchemyStarsGeneratedOriginSun>(choiceContext);
        await AddOriginCard<AlchemyStarsGeneratedOriginMoon>(choiceContext);
        await AddOriginCard<AlchemyStarsGeneratedOriginStar>(choiceContext);
    }

    protected override void OnUpgrade()
    {
        // 升级后给予强化版起源卡牌。
    }

    private async Task AddOriginCard<T>(PlayerChoiceContext choiceContext) where T : ModCardTemplate
    {
        var card = CombatState!.CreateCard<T>(Owner);
        if (IsUpgraded)
            card.UpgradeInternal();

        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
    }
}
