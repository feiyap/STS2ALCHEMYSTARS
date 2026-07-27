using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
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
/// 林影觅踪·维克：需水光能打出；消�?1 点水光能，按水属性格数量生成奇兽牌入抽牌堆，并获得额外抽牌�?/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsWaterUncommon4 : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Power;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    protected override bool IsPlayable => LightMechanic.HasWaterLightEnergy(Owner);

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        AlchemyStarsKeywordText.InlineTitleVar("StrangeAnimal", AlchemyStarsKeywordIds.StrangeAnimal),
        AlchemyStarsKeywordText.InlineTitleVar("WaterTitle", AlchemyStarsKeywordIds.Water)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.StrangeAnimal)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.StrangeAnimal)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.LightEnergy)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.AttributeCell)),
        HoverTipFactory.FromPower<AlchemyStarsVictorDrawPower>(),
        HoverTipFactory.FromCard<AlchemyStarsGeneratedStrangeAnimalFurball>(),
        HoverTipFactory.FromCard<AlchemyStarsGeneratedStrangeAnimalBica>(),
        HoverTipFactory.FromCard<AlchemyStarsGeneratedStrangeAnimalHawk>()
    ];

    public AlchemyStarsWaterUncommon4()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        LightMechanic.TryConsumeLightEnergy(Owner, [LightElement.Water]);

        var count = LightMechanic.CountWaterAttributeCells(Owner);
        for (var i = 0; i < count; i++)
        {
            var animal = CreateRandomStrangeAnimal();
            if (IsUpgraded)
                animal.UpgradeInternal();

            await CardPileCmd.AddGeneratedCardToCombat(animal, PileType.Draw, Owner);
        }

        await PowerCmd.Apply<AlchemyStarsVictorDrawPower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            this);
    }

    private CardModel CreateRandomStrangeAnimal()
    {
        var roll = Owner.RunState.Rng.CombatTargets.NextInt(3);
        return roll switch
        {
            0 => CombatState!.CreateCard<AlchemyStarsGeneratedStrangeAnimalFurball>(Owner),
            1 => CombatState!.CreateCard<AlchemyStarsGeneratedStrangeAnimalBica>(Owner),
            _ => CombatState!.CreateCard<AlchemyStarsGeneratedStrangeAnimalHawk>(Owner),
        };
    }
}
