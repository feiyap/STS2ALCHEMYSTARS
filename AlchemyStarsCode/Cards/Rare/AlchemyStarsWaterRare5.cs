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
/// 终末之龙·希罗娜：X 费军团长；造成 X / X+1 次水伤，并施加攻击次数 ×3 层龙牙印记。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsWaterRare5 : ModCardTemplate
{
    private const int BaseEnergyCost = 0;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;
    private const decimal HitDamage = 6m;
    private const int FangMultiplier = 3;
    private const int ExtraHitUpgradeBy = 1;

    protected override bool HasEnergyCostX => true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(HitDamage, ValueProp.Move),
        new RepeatVar(0),
        new IntVar("FangMult", FangMultiplier),
        AlchemyStarsKeywordText.InlineTitleVar("LegionCommanderStrength", AlchemyStarsKeywordIds.LegionCommanderStrength),
        AlchemyStarsKeywordText.InlineTitleVar("DragonFangMark", AlchemyStarsKeywordIds.DragonFangMark),
        AlchemyStarsKeywordText.InlineTitleVar("WaterTitle", AlchemyStarsKeywordIds.Water)
    ];

    protected override HashSet<CardTag> CanonicalTags => [AlchemyStarsCardTags.LegionCommander];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.LegionCommanderStrength),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.DragonFangMark)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.LegionCommanderStrength)),
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    public AlchemyStarsWaterRare5()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await AlchemyStarsCardHelpers.TryApplyLegionCommanderStat<StrengthPower>(
            choiceContext, Owner, this);

        var hits = ResolveEnergyXValue() + DynamicVars.Repeat.IntValue;
        if (hits <= 0)
            return;

        for (var i = 0; i < hits; i++)
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
        }

        if (cardPlay.Target.IsDead)
            return;

        var fangAmount = hits * DynamicVars["FangMult"].IntValue;
        if (fangAmount > 0)
        {
            await PowerCmd.Apply<AlchemyStarsDragonFangMarkPower>(
                choiceContext,
                cardPlay.Target,
                fangAmount,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(ExtraHitUpgradeBy);
    }
}
