using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 泥尊鹰：按消耗堆奇兽数量，多次造成水属性伤害。消耗�?/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsGeneratedStrangeAnimalHawk : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Token;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = false;
    private const decimal BaseDamage = 3m;

    public override CardPoolModel VisualCardPool => ModelDb.CardPool<ColorlessCardPool>();

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseDamage, ValueProp.Move),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("Hits").WithMultiplier(CountStrangeAnimalsInExhaust),
        AlchemyStarsKeywordText.InlineTitleVar("StrangeAnimal", AlchemyStarsKeywordIds.StrangeAnimal)
    ];

    protected override HashSet<CardTag> CanonicalTags => [AlchemyStarsCardTags.StrangeAnimal];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.StrangeAnimal)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.StrangeAnimal))
    ];

    public AlchemyStarsGeneratedStrangeAnimalHawk()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var hitCount = (int)((CalculatedVar)DynamicVars["Hits"]).Calculate(cardPlay.Target);
        for (var i = 0; i < hitCount; i++)
        {
            await LightMechanic.DealElementalAttackDamage(
                choiceContext,
                Owner,
                this,
                cardPlay.Target,
                DynamicVars.Damage.BaseValue,
                LightElement.Water,
                cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
    }

    private static decimal CountStrangeAnimalsInExhaust(CardModel card, Creature? _)
    {
        if (card.Owner == null)
            return 0m;

        return PileType.Exhaust.GetPile(card.Owner).Cards
            .Count(exhausted => exhausted.Tags.Contains(AlchemyStarsCardTags.StrangeAnimal));
    }
}
