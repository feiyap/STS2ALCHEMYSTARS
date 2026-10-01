using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using AlchemyStars.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 默陵之卫·希卡蕾：叠加默陵之卫；耗 1 点森光能则为本份配置收割层数。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsForestUncommon6 : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Power;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;
    private const decimal BaseHarvestPerCopy = 2m;
    private const decimal HarvestPerCopyUpgradeBy = 2m;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<AlchemyStarsHarvestConsciousnessPower>(BaseHarvestPerCopy),
        new EnergyVar(1),
        AlchemyStarsKeywordText.InlineTitleVar("ForestTitle", AlchemyStarsKeywordIds.Forest)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.HarvestConsciousness)),
        HoverTipFactory.FromPower<AlchemyStarsShikariGuardPower>(),
        HoverTipFactory.FromPower<DoomPower>()
    ];

    public AlchemyStarsForestUncommon6()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        var harvest = 0m;
        if (LightMechanic.TryConsumeLightEnergy(Owner, [LightElement.Forest]))
            harvest = DynamicVars["AlchemyStarsHarvestConsciousnessPower"].BaseValue;

        var power = await PowerCmd.Apply<AlchemyStarsShikariGuardPower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            this);

        power ??= Owner.Creature.GetPower<AlchemyStarsShikariGuardPower>();
        power?.AddCopy(harvest);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["AlchemyStarsHarvestConsciousnessPower"].UpgradeValueBy(HarvestPerCopyUpgradeBy);
    }
}
