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
/// 乌鸦信使·阿褐：获得飞行；若打出前已飞行，可耗 1 点雷光能自动打出并消耗 1 张手牌。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsThunderUncommon1 : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

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
        HoverTipFactory.FromPower<AlchemyStarsFlyingPower>()
    ];

    public AlchemyStarsThunderUncommon1()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await AlchemyStarsCardHelpers.TriggerSkillCastAnim(this);

        var hadFlying = Owner.Creature.GetPowerAmount<AlchemyStarsFlyingPower>() > 0;

        await PowerCmd.Apply<AlchemyStarsFlyingPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["AlchemyStarsFlyingPower"].BaseValue,
            Owner.Creature,
            this);

        if (!hadFlying || !LightMechanic.TryConsumeLightEnergy(Owner, [LightElement.Thunder]))
            return;

        var hand = PileType.Hand.GetPile(Owner);
        if (hand.Cards.Count == 0)
            return;

        var selected = (await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1, 1)
            {
                PretendCardsCanBePlayed = true
            },
            card => !ReferenceEquals(card, this),
            this)).FirstOrDefault();

        if (selected == null)
            return;

        await CardCmd.AutoPlay(choiceContext, selected, null);
        await CardCmd.Exhaust(choiceContext, selected);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["AlchemyStarsFlyingPower"].UpgradeValueBy(1m);
    }
}
