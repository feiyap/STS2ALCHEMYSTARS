using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
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
using AlchemyStars.Powers;
using AlchemyStars.RestSite;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 豪荣铁颚·巴顿：军团长；获得水光能与格挡，可选耗水光能向每名敌人随机施加异常。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsWaterCommon4 : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Common;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;
    private const int BaseWaterEnergyGain = 1;
    private const int WaterEnergyGainUpgradeBy = 1;
    private const decimal BaseStatusAmount = 1m;

    public override bool GainsBlock => true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("WaterLightGain", BaseWaterEnergyGain),
        new BlockVar(4m, ValueProp.Move),
        new PowerVar<AlchemyStarsTremorPower>(BaseStatusAmount),
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
        HoverTipFactory.FromPower<DexterityPower>(),
        HoverTipFactory.FromPower<AlchemyStarsTremorPower>(),
        HoverTipFactory.FromPower<AlchemyStarsFrankClawPower>(),
        HoverTipFactory.FromPower<AlchemyStarsVelvetNeedlePower>(),
        HoverTipFactory.FromPower<PoisonPower>(),
        HoverTipFactory.FromPower<AlchemyStarsDragonFangMarkPower>()
    ];

    public AlchemyStarsWaterCommon4()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await AlchemyStarsCardHelpers.TryApplyLegionCommanderStat<DexterityPower>(
            choiceContext, Owner, this);

        LightMechanic.TryGrantLightEnergyMany(
            Owner,
            LightElement.Water,
            DynamicVars["WaterLightGain"].IntValue);

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        if (!LightMechanic.TryConsumeLightEnergy(Owner, [LightElement.Water]))
            return;

        var amount = DynamicVars["AlchemyStarsTremorPower"].BaseValue;
        foreach (var enemy in CombatState!.HittableEnemies.ToList())
        {
            if (enemy.IsDead)
                continue;

            await ApplyRandomStatus(choiceContext, enemy, amount);
        }
    }

    private async Task ApplyRandomStatus(
        PlayerChoiceContext choiceContext,
        Creature enemy,
        decimal amount)
    {
        var roll = Owner.RunState.Rng.CombatTargets.NextInt(5);
        switch (roll)
        {
            case 0:
                await PowerCmd.Apply<AlchemyStarsTremorPower>(
                    choiceContext, enemy, amount, Owner.Creature, this);
                break;
            case 1:
                await PowerCmd.Apply<AlchemyStarsFrankClawPower>(
                    choiceContext, enemy, amount, Owner.Creature, this);
                break;
            case 2:
                await PowerCmd.Apply<AlchemyStarsVelvetNeedlePower>(
                    choiceContext, enemy, amount, Owner.Creature, this);
                break;
            case 3:
                await PowerCmd.Apply<PoisonPower>(
                    choiceContext, enemy, amount, Owner.Creature, this);
                break;
            default:
                await PowerCmd.Apply<AlchemyStarsDragonFangMarkPower>(
                    choiceContext, enemy, amount, Owner.Creature, this);
                break;
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["WaterLightGain"].UpgradeValueBy(WaterEnergyGainUpgradeBy);
        DynamicVars["AlchemyStarsTremorPower"].UpgradeValueBy(1m);
    }

    public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
    {
        if (player != Owner)
            return false;

        return AlchemyStarsBartonFusionRestSiteOption.TryAddOption(player, options);
    }
}
