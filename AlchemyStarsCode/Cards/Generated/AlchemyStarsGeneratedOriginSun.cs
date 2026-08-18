using System.Linq;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using AlchemyStars.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 起源·日：检视抽牌堆顶 5 张，选 1 张入手并可将最多 2 张置于牌堆底。
/// </summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class AlchemyStarsGeneratedOriginSun : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Token;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = false;
    private const int ScryCount = 5;
    private const int BottomCount = 2;

    public override bool CanBeGeneratedInCombat => false;

    public override CardPoolModel VisualCardPool => ModelDb.CardPool<ColorlessCardPool>();

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(ScryCount),
        new DynamicVar("Bottom", BottomCount)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
        HoverTipFactory.FromPower<RetainHandPower>()
    ];

    public AlchemyStarsGeneratedOriginSun()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var drawPile = PileType.Draw.GetPile(Owner);
        var options = drawPile.Cards.Take(ScryCount).ToList();
        if (options.Count > 0)
        {
            var picked = (await CardSelectCmd.FromCombatPile(
                choiceContext,
                drawPile,
                Owner,
                new CardSelectorPrefs(SelectionScreenPrompt, 0, 1),
                card => options.Contains(card))).FirstOrDefault();

            if (picked != null)
                await CardPileCmd.Add(picked, PileType.Hand);

            var remaining = options.Where(card => !ReferenceEquals(card, picked)).ToList();
            if (remaining.Count > 0)
            {
                var toBottom = (await CardSelectCmd.FromCombatPile(
                    choiceContext,
                    drawPile,
                    Owner,
                    new CardSelectorPrefs(GetSecondaryPrompt(), 0, Math.Min(BottomCount, remaining.Count)),
                    card => remaining.Contains(card))).ToList();

                if (toBottom.Count > 0)
                    await CardPileCmd.Add(toBottom, PileType.Draw, CardPilePosition.Bottom, this);
            }
        }

        await PowerCmd.Apply<RetainHandPower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }

    private LocString GetSecondaryPrompt() =>
        LocString.GetIfExists("cards", Id.Entry + ".putOnBottomPrompt")
        ?? CardSelectorPrefs.DiscardSelectionPrompt;
}
