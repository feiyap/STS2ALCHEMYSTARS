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

/// <summary>
/// 终末之龙·希罗娜：X 费；需水光能打出，消�?1 点水光能后按 X 多段水伤并叠加龙牙印记�?/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsWaterRare5 : ModCardTemplate
{
    private const int BaseEnergyCost = 0;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;
    private const decimal HitDamage = 1m;

    protected override bool HasEnergyCostX => true;

    protected override bool IsPlayable => LightMechanic.HasWaterLightEnergy(Owner);

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(HitDamage, ValueProp.Move),
        AlchemyStarsKeywordText.InlineTitleVar("DragonFangMark", AlchemyStarsKeywordIds.DragonFangMark),
        AlchemyStarsKeywordText.InlineTitleVar("WaterTitle", AlchemyStarsKeywordIds.Water)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.DragonFangMark)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.DragonFangMark)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.LightEnergy)),
        HoverTipFactory.FromPower<AlchemyStarsDragonFangMarkPower>()
    ];

    public AlchemyStarsWaterRare5()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        LightMechanic.TryConsumeLightEnergy(Owner, [LightElement.Water]);

        var x = ResolveEnergyXValue();
        if (x <= 0)
            return;

        for (var i = 0; i < x; i++)
        {
            if (cardPlay.Target.IsDead)
                break;

            await LightMechanic.DealElementalAttackDamage(
                choiceContext,
                Owner,
                this,
                cardPlay.Target,
                DynamicVars.Damage.BaseValue,
                LightElement.Water,
                cardPlay);

            await PowerCmd.Apply<AlchemyStarsDragonFangMarkPower>(
                choiceContext,
                cardPlay.Target,
                1m,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Exhaust);
    }
}
