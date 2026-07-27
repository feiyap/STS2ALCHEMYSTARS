using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 商者游行·狡木：群体森伤，单目标时伤害翻倍；回合结束时消耗森光能保留�?/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsForestCommon2 : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Common;
    private const TargetType CardTarget = TargetType.AllEnemies;
    private const bool ShowInCardLibrary = true;
    private const decimal SingleTargetDamage = 14m;
    private const decimal SingleTargetDamageUpgradeBy = 4m;

    public override bool HasTurnEndInHandEffect => true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, ValueProp.Move),
        new IntVar("SingleTargetDamage", SingleTargetDamage),
        AlchemyStarsKeywordText.InlineTitleVar("ForestTitle", AlchemyStarsKeywordIds.Forest)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Forest)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.LightEnergy))
    ];

    public AlchemyStarsForestCommon2()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var enemies = CombatState!.HittableEnemies.ToList();
        var damage = enemies.Count == 1
            ? DynamicVars["SingleTargetDamage"].BaseValue
            : DynamicVars.Damage.BaseValue;

        foreach (var enemy in enemies)
        {
            await LightMechanic.DealElementalAttackDamage(
                choiceContext,
                Owner,
                this,
                enemy,
                damage,
                LightElement.Forest,
                cardPlay);
        }
    }

    protected override Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
    {
        if (LightMechanic.TryConsumeLightEnergy(Owner, [LightElement.Forest]))
            GiveSingleTurnRetain();

        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["SingleTargetDamage"].UpgradeValueBy(SingleTargetDamageUpgradeBy);
    }
}
