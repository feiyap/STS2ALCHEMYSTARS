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
/// ????�????????????????????
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsThunderUncommon8 : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;
    private const int HitCount = 4;
    private const int MinDamage = 3;
    private const int MaxDamage = 4;
    private const int MaxDamageUpgradeBy = 1;

    protected override bool ShouldGlowGoldInternal =>
        Owner.Creature.GetPowerAmount<AlchemyStarsOverheatPower>() > 0;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(MinDamage, ValueProp.Move),
        new IntVar("Max", MaxDamage),
        new RepeatVar(HitCount),
        AlchemyStarsKeywordText.InlineTitleVar("OverheatBattleSkill", AlchemyStarsKeywordIds.OverheatBattleSkill),
        AlchemyStarsKeywordText.InlineTitleVar("ThunderTitle", AlchemyStarsKeywordIds.Thunder)
    ];

    protected override HashSet<CardTag> CanonicalTags => [AlchemyStarsCardTags.OverheatBattleSkill];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.OverheatBattleSkill)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder)),
        HoverTipFactory.FromPower<AlchemyStarsOverheatPower>()
    ];

    public AlchemyStarsThunderUncommon8()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var overheated = Owner.Creature.GetPowerAmount<AlchemyStarsOverheatPower>() > 0;
        var minDamage = DynamicVars.Damage.IntValue;
        var maxDamage = DynamicVars["Max"].IntValue;

        for (var i = 0; i < HitCount; i++)
        {
            var damage = overheated
                ? maxDamage
                : Owner.RunState.Rng.CombatTargets.NextInt(minDamage, maxDamage + 1);

            await LightMechanic.DealElementalAttackDamage(
                choiceContext,
                Owner,
                this,
                cardPlay.Target,
                damage,
                LightElement.Thunder,
                cardPlay);
        }

        var overheat = await PowerCmd.Apply<AlchemyStarsOverheatPower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            this);

        if (overheat != null)
            overheat.ScheduleRemovalAfterNextTurnEnd(Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Max"].UpgradeValueBy(MaxDamageUpgradeBy);
    }
}
