using System.Linq;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using AlchemyStars.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 起源·星：预知抽牌堆顶 5 张并自选丢弃或留下，再抽 2 张并保留其中 2 张。消耗。
/// </summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class AlchemyStarsGeneratedOriginStar : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Token;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = false;
    private const int ForesightCount = 5;
    private const int DrawCount = 2;
    private const int RetainCount = 2;

    public override bool CanBeGeneratedInCombat => false;

    public override CardPoolModel VisualCardPool => ModelDb.CardPool<ColorlessCardPool>();

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(ForesightCount),
        new DynamicVar("Draw", DrawCount),
        new DynamicVar("RetainPick", RetainCount)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromKeyword(CardKeyword.Retain)
    ];

    public AlchemyStarsGeneratedOriginStar()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var drawPile = PileType.Draw.GetPile(Owner);
        var preview = drawPile.Cards.Take(ForesightCount).ToList();
        if (preview.Count > 0)
        {
            var toDiscard = (await CardSelectCmd.FromCombatPile(
                choiceContext,
                drawPile,
                Owner,
                new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 0, preview.Count),
                card => preview.Contains(card))).ToList();

            if (toDiscard.Count > 0)
                await CardCmd.Discard(choiceContext, toDiscard);
        }

        await CardPileCmd.Draw(choiceContext, DrawCount, Owner);

        var retainSelection = (await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 0, RetainCount),
            card => !card.ShouldRetainThisTurn,
            this)).ToList();

        foreach (var card in retainSelection)
            card.GiveSingleTurnRetain();
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
