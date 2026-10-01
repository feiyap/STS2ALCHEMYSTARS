using System.Linq;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using AlchemyStars.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 烈日幽蓝·青瞳：全体水伤；未格挡获水深色格；水加成翻倍；获得蜃影。
/// </summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class AlchemyStarsQingTong : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.AllEnemies;
    private const bool ShowInCardLibrary = false;
    private const decimal BaseDamage = 8m;
    private const decimal DamageUpgradeBy = 3m;
    private const int MirageStacks = 2;

    public override bool CanBeGeneratedInCombat => false;

    public override bool CanBeGeneratedByModifiers => false;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseDamage, ValueProp.Move),
        new PowerVar<AlchemyStarsMiragePower>(MirageStacks),
        AlchemyStarsKeywordText.InlineTitleVar("WaterTitle", AlchemyStarsKeywordIds.Water),
        AlchemyStarsKeywordText.InlineTitleVar("Mirage", AlchemyStarsKeywordIds.Mirage)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.NitrogenRain),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Mirage)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.NitrogenRain)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Mirage)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.DarkCell)),
        HoverTipFactory.FromPower<AlchemyStarsMiragePower>()
    ];

    public AlchemyStarsQingTong()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (var enemy in CombatState!.HittableEnemies.ToList())
        {
            decimal unblocked;
            using (LightMechanicDamageContext.UseWaterBonusDoubled())
            using (LightMechanicDamageContext.Use(LightElement.Water))
            {
                var attack = DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                    .FromCard(this, cardPlay)
                    .Targeting(enemy);
                await attack.Execute(choiceContext);
                unblocked = attack.Results
                    .SelectMany(result => result)
                    .Sum(result => (decimal)result.UnblockedDamage);
            }

            await LightMechanic.ApplyElementalHitEffects(
                choiceContext,
                Owner,
                enemy,
                LightElement.Water,
                this);

            if (unblocked > 0m)
                LightMechanic.TryAddAttributeCell(Owner, LightElement.Water, AttributeCellKind.Dark);
        }

        await PowerCmd.Apply<AlchemyStarsMiragePower>(
            choiceContext,
            Owner.Creature,
            MirageStacks,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(DamageUpgradeBy);
    }
}
