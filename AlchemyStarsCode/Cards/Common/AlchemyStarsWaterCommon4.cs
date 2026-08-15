using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using AlchemyStars.RestSite;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 豪荣铁颚·巴顿：军团长；全体失去生命，获得水光能与覆甲。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsWaterCommon4 : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Common;
    private const TargetType CardTarget = TargetType.AllEnemies;
    private const bool ShowInCardLibrary = true;
    private const decimal HpLoss = 4m;
    private const int WaterEnergyGain = 2;
    private const decimal BasePlating = 2m;
    private const decimal PlatingUpgradeBy = 2m;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HpLossVar(HpLoss),
        new PowerVar<PlatingPower>(BasePlating),
        AlchemyStarsKeywordText.InlineTitleVar("LegionCommander", AlchemyStarsKeywordIds.LegionCommander),
        AlchemyStarsKeywordText.InlineTitleVar("WaterTitle", AlchemyStarsKeywordIds.Water)
    ];

    protected override HashSet<CardTag> CanonicalTags => [AlchemyStarsCardTags.LegionCommander];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.LegionCommander)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.LegionCommander)),
        HoverTipFactory.FromPower<PlatingPower>()
    ];

    public AlchemyStarsWaterCommon4()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await AlchemyStarsCardHelpers.TryApplyLegionCommanderStat<DexterityPower>(
            choiceContext, Owner, this);

        foreach (var enemy in CombatState!.HittableEnemies.ToList())
        {
            if (enemy.IsDead)
                continue;

            await CreatureCmd.Damage(
                choiceContext,
                enemy,
                HpLoss,
                ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
                this,
                cardPlay);
        }

        LightMechanic.TryGrantLightEnergyMany(Owner, LightElement.Water, WaterEnergyGain);

        await PowerCmd.Apply<PlatingPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["PlatingPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["PlatingPower"].UpgradeValueBy(PlatingUpgradeBy);
    }

    public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
    {
        if (player != Owner)
            return false;

        return AlchemyStarsBartonFusionRestSiteOption.TryAddOption(player, options);
    }
}
