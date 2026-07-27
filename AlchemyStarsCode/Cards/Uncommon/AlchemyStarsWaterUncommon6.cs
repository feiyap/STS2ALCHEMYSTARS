using System.Linq;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using AlchemyStars.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 安息之辉·蒂娜：从抽牌堆选牌入手后消耗自身，若干回合后回手并获得等效水格挡�?/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsWaterUncommon6 : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;
    private const int BaseReturnTurns = 1;
    private const int ReturnTurnsUpgradeBy = 1;

    public override bool GainsBlock => true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<AlchemyStarsTinaTurnStartPower>(BaseReturnTurns),
        AlchemyStarsKeywordText.InlineTitleVar("DivineHand", AlchemyStarsKeywordIds.DivineHand),
        AlchemyStarsKeywordText.InlineTitleVar("Sleepwalk", AlchemyStarsKeywordIds.Sleepwalk),
        AlchemyStarsKeywordText.InlineTitleVar("WaterTitle", AlchemyStarsKeywordIds.Water)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.DivineHand),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Sleepwalk),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.DivineHand)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Sleepwalk)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.LightEnergy)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.DarkCell)),
        HoverTipFactory.FromPower<AlchemyStarsTinaTurnStartPower>()
    ];

    public AlchemyStarsWaterUncommon6()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await TryPickFromDrawPile(choiceContext);

        var block = LightMechanic.CountEffectiveWaterCells(Owner);
        if (block > 0)
            await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(block, ValueProp.Move), cardPlay);

        var power = await PowerCmd.Apply<AlchemyStarsTinaTurnStartPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["AlchemyStarsTinaTurnStartPower"].IntValue,
            Owner.Creature,
            this);
        power?.ConfigureExhaustedCard(this);

        await CardCmd.Exhaust(choiceContext, this);
    }

    private async Task TryPickFromDrawPile(PlayerChoiceContext choiceContext)
    {
        var drawPile = PileType.Draw.GetPile(Owner);
        if (drawPile.Cards.Count == 0)
        {
            await CardPileCmd.Draw(choiceContext, 1, Owner);
            return;
        }

        var selected = (await CardSelectCmd.FromCombatPile(
            choiceContext,
            drawPile,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1))).FirstOrDefault();

        if (selected != null)
            await CardPileCmd.Add(selected, PileType.Hand);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["AlchemyStarsTinaTurnStartPower"].UpgradeValueBy(ReturnTurnsUpgradeBy);
    }
}
