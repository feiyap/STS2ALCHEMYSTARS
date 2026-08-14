using System.Collections.Generic;
using System.Threading.Tasks;
using AlchemyStars.Cards;
using AlchemyStars.Keywords;
using AlchemyStars.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using AlchemyStars.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// ????????????????????????????????/// </summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class AlchemyStarsGeneratedRebellionBurningDay : ModCardTemplate
{
    private const int BaseEnergyCost = 0;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Token;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = false;

    public override bool CanBeGeneratedInCombat => false;

    public override CardPoolModel VisualCardPool => ModelDb.CardPool<ColorlessCardPool>();

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromCard<AlchemyStarsGeneratedRebellionBurningReinhardt>(),
        HoverTipFactory.FromPower<AlchemyStarsRebellionHpPayPower>()
    ];

    public AlchemyStarsGeneratedRebellionBurningDay()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        AlchemyStarsRebellionBurningHelper.GrantRebellionBurningToAwakenedCards(Owner);

        var reinhardt = CombatState!.CreateCard<AlchemyStarsGeneratedRebellionBurningReinhardt>(Owner);
        await CardPileCmd.AddGeneratedCardToCombat(reinhardt, PileType.Hand, Owner);

        if (!IsUpgraded)
            return;

        var existing = Owner.Creature.GetPower<AlchemyStarsRebellionHpPayPower>();
        if (existing != null)
        {
            existing.ActivateFromNextTurn(Owner);
            return;
        }

        var power = await PowerCmd.Apply<AlchemyStarsRebellionHpPayPower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            this);

        power?.ActivateFromNextTurn(Owner);
    }
}
