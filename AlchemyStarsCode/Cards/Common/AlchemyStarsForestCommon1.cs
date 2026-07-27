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
/// 咕咕咕咕·多孚：造成森属性伤害；飞行时附加虚弱；升级可先消耗森光能获得飞行�?/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsForestCommon1 : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Common;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;
    private const decimal BaseDamage = 9m;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseDamage, ValueProp.Move),
        new PowerVar<WeakPower>(1m),
        AlchemyStarsKeywordText.InlineTitleVar("ForestTitle", AlchemyStarsKeywordIds.Forest)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.LightEnergy)),
        HoverTipFactory.FromPower<AlchemyStarsFlyingPower>(),
        HoverTipFactory.FromPower<WeakPower>()
    ];

    public AlchemyStarsForestCommon1()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        if (IsUpgraded &&
            LightMechanic.TryConsumeLightEnergy(Owner, [LightElement.Forest]))
        {
            await PowerCmd.Apply<AlchemyStarsFlyingPower>(
                choiceContext,
                Owner.Creature,
                1m,
                Owner.Creature,
                this);
        }

        await LightMechanic.DealElementalAttackDamage(
            choiceContext,
            Owner,
            this,
            cardPlay.Target,
            DynamicVars.Damage.BaseValue,
            LightElement.Forest,
            cardPlay);

        if (Owner.Creature.GetPowerAmount<AlchemyStarsFlyingPower>() > 0)
        {
            await PowerCmd.Apply<WeakPower>(
                choiceContext,
                cardPlay.Target,
                DynamicVars.Weak.BaseValue,
                Owner.Creature,
                this);
        }
    }
}
