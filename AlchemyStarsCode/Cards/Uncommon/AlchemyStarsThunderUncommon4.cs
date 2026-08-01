using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 巡航阵列·雷霆：军团长；需消耗雷光能打出；清除防御并造成伤害，打出后塞入超载�?/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsThunderUncommon4 : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;
    private const decimal BaseDamage = 8m;

    protected override bool IsPlayable => LightMechanic.HasThunderLightEnergy(Owner);

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseDamage, ValueProp.Move),
        AlchemyStarsKeywordText.InlineTitleVar("LegionCommander", AlchemyStarsKeywordIds.LegionCommander),
        AlchemyStarsKeywordText.InlineTitleVar("Overload", AlchemyStarsKeywordIds.Overload),
        AlchemyStarsKeywordText.InlineTitleVar("ThunderTitle", AlchemyStarsKeywordIds.Thunder)
    ];

    protected override HashSet<CardTag> CanonicalTags => [AlchemyStarsCardTags.LegionCommander];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.LegionCommander),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Overload)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder)),
        
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Overload)),
        HoverTipFactory.FromCard<AlchemyStarsGeneratedOverload>(),
        HoverTipFactory.FromPower<SlipperyPower>(),
        HoverTipFactory.FromPower<BufferPower>()
    ];

    public AlchemyStarsThunderUncommon4()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        LightMechanic.TryConsumeLightEnergy(Owner, [LightElement.Thunder]);

        if (AlchemyStarsCardHelpers.IsFirstCardPlayedThisTurn(this, Owner, CombatState))
        {
            var drawPile = PileType.Draw.GetPile(Owner);
            var legionCards = drawPile.Cards
                .Where(card => card.Tags.Contains(AlchemyStarsCardTags.LegionCommander))
                .ToList();

            if (legionCards.Count > 0)
            {
                var picked = Owner.RunState.Rng.CombatTargets.NextItem(legionCards);
                if (picked != null)
                    await CardPileCmd.Add(picked, PileType.Hand);
            }
        }

        await AlchemyStarsCardHelpers.ClearEnemyDefenses(choiceContext, cardPlay.Target);

        await LightMechanic.DealElementalAttackDamage(
            choiceContext,
            Owner,
            this,
            cardPlay.Target,
            DynamicVars.Damage.BaseValue,
            LightElement.Thunder,
            cardPlay);

        var overload = CombatState!.CreateCard<AlchemyStarsGeneratedOverload>(Owner);
        if (IsUpgraded)
        {
            overload.GrantsThunderAttackReplay = true;
            CardCmd.Upgrade(overload);
        }

        await CardPileCmd.AddGeneratedCardToCombat(overload, PileType.Draw, Owner);
    }
}
