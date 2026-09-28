using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 壮志凌云·巴顿：觉醒产物（先古，不进空裔卡池）；全体 2 次万色伤，获得万色光能并转化格子。
/// </summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class AlchemyStarsWaterRareBarton : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Ancient;
    private const int PrismaticEnergyGain = 1;
    private const TargetType CardTarget = TargetType.AllEnemies;
    private const bool ShowInCardLibrary = true;
    private const int HitCount = 2;
    private const decimal HitDamage = 4m;

    public override bool CanBeGeneratedInCombat => false;

    public override CardPoolModel VisualCardPool => ModelDb.CardPool<AlchemyStarsCardPool>();

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(HitDamage, ValueProp.Move),
        new RepeatVar(HitCount),
        new IntVar("PrismaticGain", PrismaticEnergyGain),
        AlchemyStarsKeywordText.InlineTitleVar("RebellionBurning", AlchemyStarsKeywordIds.RebellionBurning),
        AlchemyStarsKeywordText.InlineTitleVar("PrismaticTitle", AlchemyStarsKeywordIds.Prismatic),
        AlchemyStarsKeywordText.InlineTitleVar("WaterTitle", AlchemyStarsKeywordIds.Water)
    ];

    protected override HashSet<CardTag> CanonicalTags => [AlchemyStarsCardTags.RebellionBurning];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.RebellionBurning)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.RebellionBurning)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Prismatic))
    ];

    public AlchemyStarsWaterRareBarton()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await AlchemyStarsCardHelpers.TriggerSkillCastAnim(this);

        foreach (var enemy in CombatState!.HittableEnemies.ToList())
        {
            for (var i = 0; i < HitCount; i++)
            {
                await LightMechanic.DealElementalAttackDamage(
                    choiceContext,
                    Owner,
                    this,
                    enemy,
                    DynamicVars.Damage.BaseValue,
                    LightElement.Prismatic,
                    cardPlay);
            }
        }

        var prismaticGain = DynamicVars["PrismaticGain"].IntValue;
        LightMechanic.TryGrantLightEnergyMany(Owner, LightElement.Prismatic, prismaticGain);
        LightMechanic.TryConvertRandomNonElementCells(Owner, LightElement.Prismatic, prismaticGain);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["PrismaticGain"].UpgradeValueBy(1m);
        AddKeyword(CardKeyword.Retain);
    }
}
