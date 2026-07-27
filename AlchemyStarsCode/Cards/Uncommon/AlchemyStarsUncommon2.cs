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
/// 光灵召集：随机获得 1 张属性卡牌；升级后该牌本回合耗能变为 0。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsUncommon2 : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water))
    ];

    public AlchemyStarsUncommon2()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var pool = ModelDb.CardPool<AlchemyStarsCardPool>();
        var candidates = pool
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Where(IsEligibleAttributeCard)
            .ToList();

        if (candidates.Count == 0)
            return;

        var created = CardFactory.GetDistinctForCombat(
            Owner,
            candidates,
            DynamicVars.Cards.IntValue,
            Owner.RunState.Rng.CombatCardGeneration);

        foreach (var card in created)
        {
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
            if (IsUpgraded)
                card.EnergyCost.SetThisTurn(0);
        }
    }

    private static bool IsEligibleAttributeCard(CardModel card) =>
        AlchemyStarsCardHelpers.IsAttributeCard(card) &&
        card.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare &&
        card is not AlchemyStarsUncommon2;
}
