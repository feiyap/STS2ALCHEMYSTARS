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
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 弹雨慰痕·珀拉珂：可强化自身增益并翻倍敌人减益，造成伤害并获得同额格挡�?/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsThunderUncommon9 : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;

    public override bool GainsBlock => true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8m, ValueProp.Move),
        AlchemyStarsKeywordText.InlineTitleVar("ThunderTitle", AlchemyStarsKeywordIds.Thunder)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.LightEnergy))
    ];

    public AlchemyStarsThunderUncommon9()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        if (LightMechanic.TryConsumeLightEnergy(Owner, [LightElement.Thunder]))
        {
            await AlchemyStarsCardHelpers.IncrementStackableBuffs(
                choiceContext,
                Owner.Creature,
                1m,
                Owner.Creature,
                this);

            await AlchemyStarsCardHelpers.DoubleStackableDebuffs(
                choiceContext,
                cardPlay.Target,
                Owner.Creature,
                this);
        }

        decimal totalDamage;
        using (LightMechanicDamageContext.Use(LightElement.Thunder))
        {
            var attack = DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, cardPlay)
                .Targeting(cardPlay.Target);
            await attack.Execute(choiceContext);
            totalDamage = attack.Results
                .SelectMany(result => result)
                .Sum(result => (decimal)result.TotalDamage);
        }

        await LightMechanic.ApplyElementalHitEffects(
            choiceContext,
            Owner,
            cardPlay.Target,
            LightElement.Thunder,
            this);

        if (totalDamage > 0m)
            await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(totalDamage, ValueProp.Move), cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
