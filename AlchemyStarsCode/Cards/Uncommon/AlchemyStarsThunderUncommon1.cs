using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
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
/// 乌鸦信使·阿褐：获得飞行；按雷格数量获得格挡（上限 4）；已飞行则改为覆甲。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsThunderUncommon1 : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;
    private const int MaxBlockFromThunderCells = 4;

    public override bool GainsBlock => true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<AlchemyStarsFlyingPower>(1m),
        AlchemyStarsKeywordText.InlineTitleVar("ThunderTitle", AlchemyStarsKeywordIds.Thunder)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder)),
        HoverTipFactory.FromPower<AlchemyStarsFlyingPower>(),
        HoverTipFactory.FromPower<PlatingPower>()
    ];

    public AlchemyStarsThunderUncommon1()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hadFlying = Owner.Creature.GetPowerAmount<AlchemyStarsFlyingPower>() > 0;

        await PowerCmd.Apply<AlchemyStarsFlyingPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["AlchemyStarsFlyingPower"].BaseValue,
            Owner.Creature,
            this);

        var amount = Math.Min(MaxBlockFromThunderCells, LightMechanic.CountThunderAttributeCells(Owner));
        if (amount <= 0)
            return;

        if (hadFlying)
        {
            await PowerCmd.Apply<PlatingPower>(
                choiceContext,
                Owner.Creature,
                amount,
                Owner.Creature,
                this);
            return;
        }

        await CreatureCmd.GainBlock(
            Owner.Creature,
            new BlockVar(amount, ValueProp.Move),
            cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["AlchemyStarsFlyingPower"].UpgradeValueBy(1m);
    }
}
