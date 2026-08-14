using System.Linq;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 躁动炎雀·匹皮：消耗 1 张手牌，对目标施加易伤并获得飞行；已飞行则额外易伤；升级获能。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsFireCommon6 : ModCardTemplate
{
    private const int BaseEnergyCost = 0;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Common;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;
    private const decimal VulnerableAmount = 1m;
    private const decimal ExtraVulnerableWhenFlying = 1m;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<VulnerablePower>(VulnerableAmount),
        new PowerVar<AlchemyStarsFlyingPower>(1m),
        new EnergyVar(1),
        AlchemyStarsKeywordText.InlineTitleVar("FireTitle", AlchemyStarsKeywordIds.Fire)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire)),
        HoverTipFactory.FromPower<AlchemyStarsFlyingPower>(),
        HoverTipFactory.FromPower<VulnerablePower>()
    ];

    public AlchemyStarsFireCommon6()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var selectable = PileType.Hand.GetPile(Owner).Cards
            .Where(card => !ReferenceEquals(card, this))
            .ToList();
        if (selectable.Count > 0)
        {
            var selected = (await CardSelectCmd.FromHand(
                choiceContext,
                Owner,
                new CardSelectorPrefs(SelectionScreenPrompt, 1, 1),
                card => !ReferenceEquals(card, this),
                this)).ToList();

            foreach (var card in selected)
                await CardCmd.Exhaust(choiceContext, card);
        }

        var hadFlying = Owner.Creature.GetPowerAmount<AlchemyStarsFlyingPower>() > 0;
        var vulnerable = DynamicVars.Vulnerable.BaseValue;
        if (hadFlying)
            vulnerable += ExtraVulnerableWhenFlying;

        await PowerCmd.Apply<VulnerablePower>(
            choiceContext,
            cardPlay.Target,
            vulnerable,
            Owner.Creature,
            this);

        await PowerCmd.Apply<AlchemyStarsFlyingPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["AlchemyStarsFlyingPower"].BaseValue,
            Owner.Creature,
            this);

        if (IsUpgraded)
            await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
    }

    protected override void OnUpgrade()
    {
    }
}
