using System.Linq;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 梦幻泡影·摩耶：镜花水月；获得虚无，转化抽牌堆中的牌并抽牌，生成水深色格。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsWaterRare3 : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;
    private const int BaseTransformCount = 1;
    private const int TransformCountUpgradeBy = 1;
    private const int DrawCount = 1;
    private const int DarkCellGain = 2;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(DrawCount),
        new PowerVar<IntangiblePower>(1m),
        new IntVar("Transform", BaseTransformCount),
        AlchemyStarsKeywordText.InlineTitleVar("MirrorBloom", AlchemyStarsKeywordIds.MirrorBloom),
        AlchemyStarsKeywordText.InlineTitleVar("WaterTitle", AlchemyStarsKeywordIds.Water)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.MirrorBloom),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.MirrorBloom)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water)),
        
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.DarkCell)),
        HoverTipFactory.FromPower<IntangiblePower>(),
        HoverTipFactory.Static(StaticHoverTip.Transform)
    ];

    public AlchemyStarsWaterRare3()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<IntangiblePower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["IntangiblePower"].BaseValue,
            Owner.Creature,
            this);

        await TransformRandomWaterCardsInDrawPile(choiceContext);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);

        for (var i = 0; i < DarkCellGain; i++)
            LightMechanic.TryAddAttributeCell(Owner, LightElement.Water, AttributeCellKind.Dark);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Transform"].UpgradeValueBy(TransformCountUpgradeBy);
    }

    private async Task TransformRandomWaterCardsInDrawPile(PlayerChoiceContext choiceContext)
    {
        var drawPile = PileType.Draw.GetPile(Owner);
        var maxCount = DynamicVars["Transform"].IntValue;
        var candidates = drawPile.Cards.Where(card => card.IsTransformable).ToList();
        if (candidates.Count == 0)
            return;

        var selected = (await CardSelectCmd.FromCombatPile(
            choiceContext,
            drawPile,
            Owner,
            new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 0, maxCount),
            card => candidates.Contains(card))).ToList();

        foreach (var card in selected)
        {
            var replacement = CreateRandomWaterCard();
            await CardCmd.Transform(card, replacement);
        }
    }

    private CardModel CreateRandomWaterCard()
    {
        var roll = Owner.RunState.Rng.CombatTargets.NextInt(9);
        return roll switch
        {
            0 => CombatState!.CreateCard<AlchemyStarsWaterCommon1>(Owner),
            1 => CombatState!.CreateCard<AlchemyStarsWaterCommon2>(Owner),
            2 => CombatState!.CreateCard<AlchemyStarsWaterCommon3>(Owner),
            3 => CombatState!.CreateCard<AlchemyStarsWaterCommon4>(Owner),
            4 => CombatState!.CreateCard<AlchemyStarsWaterCommon5>(Owner),
            5 => CombatState!.CreateCard<AlchemyStarsWaterCommon6>(Owner),
            6 => CombatState!.CreateCard<AlchemyStarsWaterCommon7>(Owner),
            7 => CombatState!.CreateCard<AlchemyStarsWaterCommon8>(Owner),
            _ => CombatState!.CreateCard<AlchemyStarsWaterCommon9>(Owner),
        };
    }
}
