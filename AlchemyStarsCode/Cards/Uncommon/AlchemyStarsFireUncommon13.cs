using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Extensions;
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
/// 圣钉·祭礼：随机对全体战斗人员造成火伤，命中敌人治疗队友，命中队友给予力量。多人模式。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsFireUncommon13 : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;
    private const int HitCount = 5;
    private const decimal HitDamage = 2m;
    private const decimal StrengthGain = 1m;

    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(HitDamage, ValueProp.Move),
        new RepeatVar(HitCount),
        new PowerVar<StrengthPower>(StrengthGain),
        AlchemyStarsKeywordText.InlineTitleVar("FireTitle", AlchemyStarsKeywordIds.Fire)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Ethereal,
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Ethereal),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire)),
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    public AlchemyStarsFireUncommon13()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
            return;

        for (var i = 0; i < HitCount; i++)
        {
            var target = PickRandomCombatant();
            if (target == null)
                break;

            decimal actualDamage;
            using (LightMechanicDamageContext.Use(LightElement.Fire))
            {
                var attack = DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                    .FromCard(this, cardPlay)
                    .Targeting(target);
                await attack.Execute(choiceContext);
                actualDamage = attack.Results
                    .SelectMany(result => result)
                    .Sum(result => (decimal)result.TotalDamage);
            }

            await LightMechanic.ApplyElementalHitEffects(
                choiceContext,
                Owner,
                target,
                LightElement.Fire,
                this);

            if (actualDamage <= 0m)
                continue;

            if (target.IsPlayer)
            {
                await PowerCmd.Apply<StrengthPower>(
                    choiceContext,
                    target,
                    DynamicVars.Strength.BaseValue,
                    Owner.Creature,
                    this);
            }
            else
            {
                foreach (var ally in CombatState.PlayerCreatures
                             .Where(creature => creature.IsAlive && creature.IsPlayer))
                {
                    await CreatureCmd.Heal(ally, actualDamage);
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
    }

    private Creature? PickRandomCombatant()
    {
        var combatants = CombatState!.PlayerCreatures
            .Where(creature => creature.IsAlive)
            .Concat(CombatState.HittableEnemies)
            .ToList();

        if (combatants.Count == 0)
            return null;

        return Owner.RunState.Rng.CombatTargets.NextItem(combatants);
    }
}
