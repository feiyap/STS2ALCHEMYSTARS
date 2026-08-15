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
/// 绿晶巨角·奥斐娜：弃牌获能并下回合抽牌；可消耗森光能添加随机属性强化格。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsForestCommon7 : ModCardTemplate
{
    private const int BaseEnergyCost = 0;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Common;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;
    private const int BaseMaxDiscardCount = 1;
    private const int MaxDiscardCountUpgradeBy = 1;
    private const int EnergyGain = 1;
    private const int BaseDrawCount = 1;
    private const int DrawCountUpgradeBy = 1;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(EnergyGain),
        new IntVar("Discard", BaseMaxDiscardCount),
        new CardsVar(BaseDrawCount)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest))
    ];

    public AlchemyStarsForestCommon7()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var maxDiscard = DynamicVars["Discard"].IntValue;
        var discarded = (await CardSelectCmd.FromHandForDiscard(
            choiceContext,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 0, maxDiscard),
            card => !ReferenceEquals(card, this),
            this)).ToList();

        if (discarded.Count > 0)
            await CardCmd.Discard(choiceContext, discarded);

        await PlayerCmd.GainEnergy(EnergyGain, Owner);

        var drawCount = DynamicVars.Cards.IntValue;
        var drawPower = await PowerCmd.Apply<AlchemyStarsOfinaDrawPower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            this);
        drawPower?.Configure(drawCount);

        if (LightMechanic.TryConsumeLightEnergy(Owner, [LightElement.Forest]))
        {
            var elements = LightElementExtensions.BaseElements;
            var element = elements[Owner.RunState.Rng.Niche.NextInt(elements.Length)];
            LightMechanic.TryAddAttributeCell(Owner, element, AttributeCellKind.Enhanced);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Discard"].UpgradeValueBy(MaxDiscardCountUpgradeBy);
        DynamicVars.Cards.UpgradeValueBy(DrawCountUpgradeBy);
    }
}
