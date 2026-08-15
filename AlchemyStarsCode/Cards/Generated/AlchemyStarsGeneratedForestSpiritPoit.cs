using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// ??�???????????????????
/// </summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class AlchemyStarsGeneratedForestSpiritPoit : ModCardTemplate
{
    private const int BaseEnergyCost = 0;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Token;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = false;
    private const int DrawCount = 1;
    private const int EnergyGain = 2;
    private const int EmeraldMarkAmount = 1;

    public override bool CanBeGeneratedInCombat => false;

    public override CardPoolModel VisualCardPool => ModelDb.CardPool<ColorlessCardPool>();

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(DrawCount),
        new EnergyVar(EnergyGain),
        new PowerVar<AlchemyStarsEmeraldMarkPower>(EmeraldMarkAmount)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.EmeraldMark)),
        HoverTipFactory.FromPower<AlchemyStarsEmeraldMarkPower>(),
        HoverTipFactory.FromPower<AlchemyStarsFlyingPower>()
    ];

    public AlchemyStarsGeneratedForestSpiritPoit()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
        await PowerCmd.Apply<AlchemyStarsEmeraldMarkPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["AlchemyStarsEmeraldMarkPower"].BaseValue,
            Owner.Creature,
            this);

        if (IsUpgraded)
        {
            await PowerCmd.Apply<AlchemyStarsFlyingPower>(
                choiceContext,
                Owner.Creature,
                1m,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        // ??????? 1 ????
    }
}
