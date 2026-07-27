using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 凌野之鹰·蕾切尔：侦察者；转化属性格并可回收弃牌堆雷牌�?/// </summary>
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
        new RepeatVar(2),
        new CardsVar(1),
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
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.LightEnergy)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.AttributeCell)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.DarkCell)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Overload)),
        HoverTipFactory.FromCard<AlchemyStarsGeneratedOverload>()
    ];

    public AlchemyStarsThunderUncommon5()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (await AlchemyStarsCardHelpers.TryConsumeOverloadFromHand(choiceContext, Owner))
            await PlayerCmd.GainEnergy(OverloadEnergyGain, Owner);

        var convertCount = DynamicVars.Repeat.IntValue;
        var (_, darkCreated) = LightMechanic.TryConvertRandomThunderCellsWithDark(Owner, convertCount);
        if (darkCreated > 0 && LightMechanic.TryConsumeLightEnergy(Owner, [LightElement.Thunder]))
        {
            var retrieveCount = DynamicVars.Cards.IntValue;
            for (var i = 0; i < retrieveCount; i++)
            {
                var thunderCards = PileType.Discard.GetPile(Owner).Cards
                    .Where(AlchemyStarsCardHelpers.HasThunderKeyword)
                    .ToList();

                if (thunderCards.Count == 0)
                    break;

                var picked = Owner.RunState.Rng.CombatTargets.NextItem(thunderCards);
                if (picked != null)
                    await CardPileCmd.Add(picked, PileType.Hand);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1m);
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}
