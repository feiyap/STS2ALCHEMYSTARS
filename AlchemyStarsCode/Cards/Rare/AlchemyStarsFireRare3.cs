using System.Linq;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using AlchemyStars.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 渊博古典·维多利亚：消耗火光能整理抽牌堆并获得至高宝典。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsFireRare3 : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Power;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;
    private const int RequiredFireLightEnergy = 2;
    private const int ForesightCount = 5;
    private const int PickCount = 2;

    protected override bool IsPlayable =>
        LightMechanic.HasFireLightEnergyCount(Owner, RequiredFireLightEnergy);

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(ForesightCount),
        new DynamicVar("Pick", PickCount),
        AlchemyStarsKeywordText.InlineTitleVar("SupremeCodex", AlchemyStarsKeywordIds.SupremeCodex),
        AlchemyStarsKeywordText.InlineTitleVar("FireTitle", AlchemyStarsKeywordIds.Fire)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.SupremeCodex)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.SupremeCodex)),
        
        HoverTipFactory.FromPower<AlchemyStarsSupremeCodexPower>()
    ];

    public AlchemyStarsFireRare3()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        LightMechanic.TryConsumeLightEnergy(
            Owner,
            [LightElement.Fire, LightElement.Fire]);

        var toBottom = (await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1, 1),
            card => !ReferenceEquals(card, this),
            this)).FirstOrDefault();

        if (toBottom != null)
            await CardPileCmd.Add(toBottom, PileType.Draw, CardPilePosition.Bottom, this);

        var drawPile = PileType.Draw.GetPile(Owner);
        var options = drawPile.Cards.Take(ForesightCount).ToList();
        if (options.Count > 0)
        {
            var picked = (await CardSelectCmd.FromCombatPile(
                choiceContext,
                drawPile,
                Owner,
                new CardSelectorPrefs(SelectionScreenPrompt, 0, PickCount),
                card => options.Contains(card))).ToList();

            foreach (var card in picked)
                await CardPileCmd.Add(card, PileType.Hand);
        }

        await PowerCmd.Apply<AlchemyStarsSupremeCodexPower>(
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
}
