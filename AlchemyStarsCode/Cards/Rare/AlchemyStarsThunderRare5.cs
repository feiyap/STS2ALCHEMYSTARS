using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
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
/// ����֮�����������£��������ϣ�Ⱥ�����˲�ʩ�ӵ۹�������
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsThunderRare5 : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.AllEnemies;
    private const bool ShowInCardLibrary = true;
    private const decimal HitDamage = 9m;
    private const int BaseHitCount = 2;
    private const int ImperialThunderApplyAmount = 99;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(HitDamage, ValueProp.Move),
        new RepeatVar(BaseHitCount),
        new PowerVar<AlchemyStarsImperialThunderPower>(ImperialThunderApplyAmount),
        AlchemyStarsKeywordText.InlineTitleVar("RighteousMajesty", AlchemyStarsKeywordIds.RighteousMajesty),
        AlchemyStarsKeywordText.InlineTitleVar("ThunderTitle", AlchemyStarsKeywordIds.Thunder)
    ];

    protected override HashSet<CardTag> CanonicalTags => [AlchemyStarsCardTags.RighteousMajesty];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.RighteousMajesty)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder)),
        
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.RighteousMajesty)),
        HoverTipFactory.FromPower<AlchemyStarsImperialThunderPower>()
    ];

    public AlchemyStarsThunderRare5()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        LightMechanic.TryGrantLightEnergy(Owner, LightElement.Thunder);

        foreach (var enemy in CombatState!.HittableEnemies.ToList())
            await AttackEnemyWithRighteousMajesty(choiceContext, enemy, cardPlay);

        await PowerCmd.Apply<AlchemyStarsImperialThunderPower>(
            choiceContext,
            CombatState.HittableEnemies,
            DynamicVars["AlchemyStarsImperialThunderPower"].BaseValue,
            Owner.Creature,
            this);
    }

    private async Task AttackEnemyWithRighteousMajesty(
        PlayerChoiceContext choiceContext,
        Creature enemy,
        CardPlay cardPlay)
    {
        var hitCount = DynamicVars.Repeat.IntValue;
        if (enemy.GetPowerAmount<AlchemyStarsImperialThunderPower>() > 0)
            hitCount += 1;

        for (var i = 0; i < hitCount; i++)
        {
            if (enemy.IsDead)
                break;

            await LightMechanic.DealElementalAttackDamage(
                choiceContext,
                Owner,
                this,
                enemy,
                DynamicVars.Damage.BaseValue,
                LightElement.Thunder,
                cardPlay);
        }

        if (enemy.GetPowerAmount<AlchemyStarsImperialThunderPower>() > 0)
        {
            var imperialThunder = enemy.GetPower<AlchemyStarsImperialThunderPower>();
            if (imperialThunder != null)
            {
                await PowerCmd.Decrement(imperialThunder);

                if (IsUpgraded && !enemy.IsDead)
                {
                    var bonus = enemy.MaxHp * 0.05m;
                    if (bonus > 0m)
                    {
                        await CreatureCmd.Damage(
                            choiceContext,
                            enemy,
                            bonus,
                            ValueProp.Unblockable | ValueProp.Unpowered,
                            this,
                            cardPlay);
                    }
                }
            }
        }
    }
}
