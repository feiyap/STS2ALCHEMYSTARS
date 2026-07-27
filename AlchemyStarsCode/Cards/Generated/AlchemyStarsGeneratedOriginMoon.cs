using System.Linq;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using AlchemyStars.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 起源·月：检视抽牌堆底 5 张，选 1 张入手并可将最多 2 张置于牌堆顶。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsGeneratedOriginMoon : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Token;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = false;
    private const int ScryCount = 5;
    private const int TopCount = 2;

    public override CardPoolModel VisualCardPool => ModelDb.CardPool<ColorlessCardPool>();

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(ScryCount),
        new DynamicVar("Top", TopCount)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
        HoverTipFactory.FromPower<RetainHandPower>()
    ];

    public AlchemyStarsGeneratedOriginMoon()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PerformBottomScry(choiceContext, ScryCount, TopCount);
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

    internal static async Task PerformBottomScry(
        PlayerChoiceContext choiceContext,
        int scryCount,
        int topCount,
        CardModel? source = null,
        Player? owner = null)
    {
        owner ??= source?.Owner ?? throw new InvalidOperationException("缺少卡牌所属玩家。");
        var drawPile = PileType.Draw.GetPile(owner);
        var options = drawPile.Cards.TakeLast(scryCount).ToList();
        if (options.Count == 0)
            return;

        var picked = (await CardSelectCmd.FromCombatPile(
            choiceContext,
            drawPile,
            owner,
            new CardSelectorPrefs(source?.SelectionScreenPrompt ?? CardSelectorPrefs.TransformSelectionPrompt, 1),
            card => options.Contains(card))).FirstOrDefault();

        if (picked != null)
            await CardPileCmd.Add(picked, PileType.Hand);

        var remaining = options.Where(card => !ReferenceEquals(card, picked)).ToList();
        if (remaining.Count == 0 || topCount <= 0)
            return;

        var toTop = (await CardSelectCmd.FromCombatPile(
            choiceContext,
            drawPile,
            owner,
            new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 0, topCount),
            card => remaining.Contains(card))).ToList();

        if (toTop.Count > 0)
            await CardPileCmd.Add(toTop, PileType.Draw, CardPilePosition.Top, source);
    }
}
