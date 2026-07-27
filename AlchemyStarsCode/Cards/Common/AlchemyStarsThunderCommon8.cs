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
/// ѩ����Ӱ�����ţ���÷��в�����������˺������ǰ�ѷ������˺���ø񵲡�
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsThunderCommon8 : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Common;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, ValueProp.Move),
        new PowerVar<AlchemyStarsFlyingPower>(1m),
        AlchemyStarsKeywordText.InlineTitleVar("ThunderTitle", AlchemyStarsKeywordIds.Thunder)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder)),
        HoverTipFactory.FromPower<AlchemyStarsFlyingPower>()
    ];

    public AlchemyStarsThunderCommon8() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var hadFlying = Owner.Creature.GetPowerAmount<AlchemyStarsFlyingPower>() > 0;

        await PowerCmd.Apply<AlchemyStarsFlyingPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["AlchemyStarsFlyingPower"].BaseValue,
            Owner.Creature,
            this);

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

        if (hadFlying && totalDamage > 0m)
            await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(totalDamage, ValueProp.Move), cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
