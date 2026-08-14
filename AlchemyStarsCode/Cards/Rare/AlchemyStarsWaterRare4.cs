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

/// <summary>
/// 水形之音·克娜莉：影镇茶话会；获得梦魇荆棘，群体水伤并施加颤栗，可触发低阈值眩晕�?/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsWaterRare4 : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.AllEnemies;
    private const bool ShowInCardLibrary = true;
    private const decimal BaseDamage = 6m;
    private const decimal BaseTremorAmount = 1m;
    private const decimal TremorAmountUpgradeBy = 1m;
    private const int StunThreshold = 4;
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseDamage, ValueProp.Move),
        new PowerVar<AlchemyStarsTremorPower>(BaseTremorAmount),
        new PowerVar<AlchemyStarsNightmareThornPower>(1m),
        AlchemyStarsKeywordText.InlineTitleVar("ShadowTownTeaParty", AlchemyStarsKeywordIds.ShadowTownTeaParty),
        AlchemyStarsKeywordText.InlineTitleVar("NightmareThorn", AlchemyStarsKeywordIds.NightmareThorn),
        AlchemyStarsKeywordText.InlineTitleVar("WaterTitle", AlchemyStarsKeywordIds.Water)
    ];

    protected override HashSet<CardTag> CanonicalTags => [AlchemyStarsCardTags.ShadowTownTeaParty];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.NightmareThorn),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.ShadowTownTeaParty)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water)),
        HoverTipFactory.FromPower<AlchemyStarsTremorPower>()
    ];

    public AlchemyStarsWaterRare4()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<AlchemyStarsNightmareThornPower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            this);

        await AlchemyStarsCardHelpers.TryTriggerTeaPartyOnPlay(choiceContext, this, Owner);

        var tremorAmount = DynamicVars["AlchemyStarsTremorPower"].BaseValue;
        foreach (var enemy in CombatState!.HittableEnemies.ToList())
        {
            await PowerCmd.Apply<AlchemyStarsTremorPower>(
                choiceContext,
                enemy,
                tremorAmount,
                Owner.Creature,
                this);

            await LightMechanic.DealElementalAttackDamage(
                choiceContext,
                Owner,
                this,
                enemy,
                DynamicVars.Damage.BaseValue,
                LightElement.Water,
                cardPlay);

            await AlchemyStarsTremorPower.TryTriggerStunThreshold(choiceContext, enemy, StunThreshold);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["AlchemyStarsTremorPower"].UpgradeValueBy(TremorAmountUpgradeBy);
    }
}
