using System.Linq;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 凌野之鹰·蕾切尔：侦察者；添加深色雷格并获得同量覆甲，可自选弃牌堆雷牌回手。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsThunderUncommon5 : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;
    private const int OverloadEnergyGain = 2;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(OverloadEnergyGain),
        new RepeatVar(1),
        AlchemyStarsKeywordText.InlineTitleVar("Scout", AlchemyStarsKeywordIds.Scout),
        AlchemyStarsKeywordText.InlineTitleVar("ThunderTitle", AlchemyStarsKeywordIds.Thunder)
    ];

    protected override HashSet<CardTag> CanonicalTags => [AlchemyStarsCardTags.Scout];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Scout)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.DarkCell)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Overload)),
        HoverTipFactory.FromCard<AlchemyStarsGeneratedOverload>(),
        HoverTipFactory.FromPower<PlatingPower>()
    ];

    public AlchemyStarsThunderUncommon5()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (await AlchemyStarsCardHelpers.TryConsumeOverloadFromHand(choiceContext, Owner))
            await PlayerCmd.GainEnergy(OverloadEnergyGain, Owner);

        LightMechanic.TryAddDarkThunderCells(Owner, DynamicVars.Repeat.IntValue);

        var plating = LightMechanic.CountThunderAttributeCells(Owner);
        if (plating > 0)
        {
            await PowerCmd.Apply<PlatingPower>(
                choiceContext,
                Owner.Creature,
                plating,
                Owner.Creature,
                this);
        }

        if (!LightMechanic.TryConsumeLightEnergy(Owner, [LightElement.Thunder]))
            return;

        var discardPile = PileType.Discard.GetPile(Owner);
        if (!discardPile.Cards.Any(AlchemyStarsCardHelpers.HasThunderKeyword))
            return;

        var selected = (await CardSelectCmd.FromCombatPile(
            choiceContext,
            discardPile,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1),
            AlchemyStarsCardHelpers.HasThunderKeyword)).FirstOrDefault();

        if (selected == null)
            return;

        selected.EnergyCost.SetThisTurn(0);
        await CardPileCmd.Add(selected, PileType.Hand);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1m);
    }
}
