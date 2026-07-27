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
using AlchemyStars.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 毒脉异变·丽贝卡：花海毒池；需水光能打出，消耗全部水光能并造成群体水伤与等量中毒�?/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsWaterUncommon3 : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
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
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.LightEnergy)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.AttributeCell)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.DarkCell)),
        HoverTipFactory.FromPower<PoisonPower>(),
        HoverTipFactory.FromPower<AlchemyStarsPoisonFlowerPoolPower>()
    ];

    public AlchemyStarsWaterUncommon3()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ConsumeAllWaterLightEnergyOnPlay(Owner);

        var damage = DynamicVars.Damage.BaseValue;
        foreach (var enemy in CombatState!.HittableEnemies.ToList())
        {
            await LightMechanic.DealElementalAttackDamage(
                choiceContext,
                Owner,
                this,
                enemy,
                damage,
                LightElement.Water,
                cardPlay);

            await PowerCmd.Apply<PoisonPower>(
                choiceContext,
                enemy,
                damage,
                Owner.Creature,
                this);

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
    /// 消耗全部水光能；若拥有花海毒池则生成的属性格为深色格�?    /// </summary>
    private static void ConsumeAllWaterLightEnergyOnPlay(Player player)
    {
        var state = LightMechanic.GetActiveState(player);
        if (state == null)
            return;

        var hasPoisonPool = player.Creature.GetPowerAmount<AlchemyStarsPoisonFlowerPoolPower>() > 0;
        var waterEnergy = state.LightEnergy.Items
            .Where(item => LightElementExtensions.Matches(LightElement.Water, item))
            .ToList();
        if (waterEnergy.Count == 0)
            return;

        var remaining = state.LightEnergy.Items
            .Where(item => !LightElementExtensions.Matches(LightElement.Water, item))
            .ToList();
        state.LightEnergy.ReplaceAll(remaining);

        foreach (var element in waterEnergy)
        {
            var kind = hasPoisonPool
                ? AlchemyStarsPoisonFlowerPoolPower.ResolveSpawnKind(AttributeCellKind.Normal)
                : AttributeCellKind.Normal;
            state.AddAttributeCell(element, kind);
        }

        LightMechanicUiBootstrap.RefreshForPlayer(player);
        LightMechanic.NotifyLightEnergyConsumed(player, waterEnergy);
    }
}
