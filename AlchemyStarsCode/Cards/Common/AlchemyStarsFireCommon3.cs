using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 急火追光·小不点：获得火光能、抽牌，小概率生成火棱镜格，并将灼烧放入手牌。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsFireCommon3 : ModCardTemplate
{
    private const int BaseEnergyCost = 0;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Common;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;
    private const int PrismChancePercent = 25;
    private const int BurnGain = 1;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1),
        AlchemyStarsKeywordText.InlineTitleVar("FireTitle", AlchemyStarsKeywordIds.Fire)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.PrismCell)),
        HoverTipFactory.FromCard<Burn>()
    ];

    public AlchemyStarsFireCommon3()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        LightMechanic.TryGrantLightEnergy(Owner, LightElement.Fire);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);

        if (Owner.RunState.Rng.CombatTargets.NextInt(100) < PrismChancePercent)
            LightMechanic.TryAddAttributeCell(Owner, LightElement.Fire, AttributeCellKind.Prism);

        await CardPileCmd.AddToCombatAndPreview<Burn>(
            Owner.Creature,
            PileType.Hand,
            BurnGain,
            null);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}
