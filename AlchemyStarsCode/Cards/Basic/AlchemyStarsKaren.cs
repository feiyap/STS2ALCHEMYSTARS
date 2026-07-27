using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
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

[RegisterCard(typeof(AlchemyStarsCardPool))]
[RegisterCharacterStarterCard(typeof(AlchemyStarsCharacter), 1)]
[RegisterArchaicToothTranscendence(typeof(AlchemyStarsKarenBrightSoul))]
public sealed class AlchemyStarsKaren : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Basic;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1),
        AlchemyStarsKeywordText.InlineTitleVar("HighCourtGuard", AlchemyStarsKeywordIds.HighCourtGuard),
        AlchemyStarsKeywordText.InlineTitleVar("WaterTitle", AlchemyStarsKeywordIds.Water),
        new BlockVar("KarenBlock", 6m, ValueProp.Move)
    ];

    protected override HashSet<CardTag> CanonicalTags => [AlchemyStarsCardTags.HighCourtGuard];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.HighCourtGuard)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.LightEnergy)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.AttributeCell))
    ];

    public AlchemyStarsKaren() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (HasOtherHighCourtGuardInHand())
            await PlayerCmd.GainEnergy(1, Owner);

        LightMechanic.TryGrantLightEnergy(Owner, LightElement.Water);

        if (IsUpgraded)
            LightMechanic.TryAddAttributeCell(Owner, LightElement.Water);

        await PowerCmd.Apply<AlchemyStarsKarenGuardPower>(
            choiceContext,
            Owner.Creature,
            1,
            Owner.Creature,
            this);
    }

    private bool HasOtherHighCourtGuardInHand()
    {
        var hand = Owner.PlayerCombatState?.Hand.Cards;
        if (hand == null)
            return false;

        return hand.Any(card => !ReferenceEquals(card, this) && card.Tags.Contains(AlchemyStarsCardTags.HighCourtGuard));
    }
}