using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 毒脉异变·丽贝卡：花海毒池；需消耗 1 点水光能打出，造成群体水伤与等量中毒。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsWaterUncommon3 : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.AllEnemies;
    private const bool ShowInCardLibrary = true;
    private const decimal BaseDamage = 6m;
    private const int UpgradeDebuffAmount = 2;

    protected override bool IsPlayable => LightMechanic.HasWaterLightEnergy(Owner);

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseDamage, ValueProp.Move),
        AlchemyStarsKeywordText.InlineTitleVar("PoisonFlowerPool", AlchemyStarsKeywordIds.PoisonFlowerPool),
        AlchemyStarsKeywordText.InlineTitleVar("WaterTitle", AlchemyStarsKeywordIds.Water)
    ];

    protected override HashSet<CardTag> CanonicalTags => [AlchemyStarsCardTags.PoisonFlowerPool];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.PoisonFlowerPool)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.PoisonFlowerPool)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.DarkCell)),
        HoverTipFactory.FromPower<PoisonPower>()
    ];

    public AlchemyStarsWaterUncommon3()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ConsumeOneWaterLightEnergyAsDarkCell(Owner);

        var baseDamage = DynamicVars.Damage.BaseValue;
        foreach (var enemy in CombatState!.HittableEnemies.ToList())
        {
            decimal actualDamage;
            using (LightMechanicDamageContext.Use(LightElement.Water))
            {
                var attack = DamageCmd.Attack(baseDamage)
                    .FromCard(this, cardPlay)
                    .Targeting(enemy);
                await attack.Execute(choiceContext);
                actualDamage = attack.Results
                    .SelectMany(result => result)
                    .Sum(result => (decimal)result.TotalDamage);
            }

            await LightMechanic.ApplyElementalHitEffects(
                choiceContext,
                Owner,
                enemy,
                LightElement.Water,
                this);

            if (actualDamage > 0m)
            {
                await PowerCmd.Apply<PoisonPower>(
                    choiceContext,
                    enemy,
                    actualDamage,
                    Owner.Creature,
                    this);
            }

            if (IsUpgraded)
            {
                await AlchemyStarsCardHelpers.TryApplyRandomDebuff(
                    choiceContext,
                    enemy,
                    UpgradeDebuffAmount,
                    Owner.Creature,
                    this);
            }
        }
    }

    /// <summary>
    /// 消耗 1 点水光能；花海毒池使生成的属性格为深色格。
    /// </summary>
    private static void ConsumeOneWaterLightEnergyAsDarkCell(Player player)
    {
        var state = LightMechanic.GetActiveState(player);
        if (state == null)
            return;

        if (!state.LightEnergy.TryConsumeManyFromFront([LightElement.Water], out var consumed))
            return;

        foreach (var element in consumed)
            state.AddAttributeCell(element, AttributeCellKind.Dark);

        LightMechanicUiBootstrap.RefreshForPlayer(player);
        LightMechanic.NotifyLightEnergyConsumed(player, consumed);
    }
}
