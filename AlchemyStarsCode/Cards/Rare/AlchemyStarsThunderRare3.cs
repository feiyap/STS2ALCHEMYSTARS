using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 启明之光·莱因哈特：贯穿之星；段数随本回合雷伤次数增长。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsThunderRare3 : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;
    private const decimal BaseDamage = 5m;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseDamage, ValueProp.Move),
        AlchemyStarsKeywordText.InlineTitleVar("PenetratingStar", AlchemyStarsKeywordIds.PenetratingStar),
        AlchemyStarsKeywordText.InlineTitleVar("ThunderTitle", AlchemyStarsKeywordIds.Thunder)
    ];

    protected override HashSet<CardTag> CanonicalTags => [AlchemyStarsCardTags.PenetratingStar];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.PenetratingStar)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.LightEnergy)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.PenetratingStar)),
        HoverTipFactory.FromCard<AlchemyStarsGeneratedRebellionBurningDay>(),
        HoverTipFactory.FromCard<AlchemyStarsGeneratedRebellionBurningReinhardt>()
    ];

    public AlchemyStarsThunderRare3()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        if (IsUpgraded)
            await TryGrantRebellionBurningDayIfEmptiedThunderLightAsync(choiceContext);

        var hitCount = 1 + LightMechanic.GetThunderDamageDealtThisTurn(Owner);
        for (var i = 0; i < hitCount; i++)
        {
            if (cardPlay.Target.IsDead)
                break;

            await LightMechanic.DealPenetratingElementalAttackDamage(
                choiceContext,
                Owner,
                this,
                cardPlay.Target,
                DynamicVars.Damage.BaseValue,
                LightElement.Thunder,
                cardPlay);
        }
    }

    private async Task TryGrantRebellionBurningDayIfEmptiedThunderLightAsync(
        PlayerChoiceContext choiceContext)
    {
        if (LightMechanic.CountThunderLightEnergy(Owner) < 2)
            return;

        if (!LightMechanic.TryConsumeLightEnergy(Owner, [LightElement.Thunder, LightElement.Thunder]))
            return;

        if (LightMechanic.CountThunderLightEnergy(Owner) != 0)
            return;

        var card = CombatState!.CreateCard<AlchemyStarsGeneratedRebellionBurningDay>(Owner);
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
        await Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        // 升级效果为消耗雷光能后归零时获得反叛灼燃之日。
    }
}
