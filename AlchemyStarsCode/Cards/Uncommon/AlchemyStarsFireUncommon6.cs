using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using AlchemyStars.Characters;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 初始之龙·莉奥娜：龙息轰鸣；消耗全部状态牌加成伤害，多段随机火攻，可选额外火伤。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsFireUncommon6 : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.RandomEnemy;
    private const bool ShowInCardLibrary = true;
    private const int HitCount = 5;
    private const decimal DamageBonusPerStatus = 0.05m;
    private const decimal LostHpBonusRate = 0.15m;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3m, ValueProp.Move),
        new RepeatVar(HitCount),
        AlchemyStarsKeywordText.InlineTitleVar("DragonBreathRoar", AlchemyStarsKeywordIds.DragonBreathRoar),
        AlchemyStarsKeywordText.InlineTitleVar("FireTitle", AlchemyStarsKeywordIds.Fire)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.DragonBreathRoar)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.DragonBreathRoar)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.LightEnergy))
    ];

    public AlchemyStarsFireUncommon6()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var statuses = CollectAllStatusCards().ToList();
        foreach (var status in statuses)
            await CardCmd.Exhaust(choiceContext, status);

        var damageMultiplier = 1m + statuses.Count * DamageBonusPerStatus;
        var hitDamage = DynamicVars.Damage.BaseValue * damageMultiplier;

        for (var i = 0; i < HitCount; i++)
        {
            var target = PickRandomEnemy();
            if (target == null)
                break;

            await LightMechanic.DealElementalAttackDamage(
                choiceContext,
                Owner,
                this,
                target,
                hitDamage,
                LightElement.Fire,
                cardPlay);
        }

        if (LightMechanic.TryConsumeLightEnergy(Owner, [LightElement.Fire]))
        {
            var lostHp = Owner.Creature.MaxHp - Owner.Creature.CurrentHp;
            var bonusDamage = (int)System.Math.Ceiling(lostHp * LostHpBonusRate);
            if (bonusDamage > 0)
            {
                var bonusTarget = PickRandomEnemy();
                if (bonusTarget != null)
                {
                    await LightMechanic.DealElementalAttackDamage(
                        choiceContext,
                        Owner,
                        this,
                        bonusTarget,
                        bonusDamage,
                        LightElement.Fire,
                        cardPlay);
                }
            }
        }
    }

    private IEnumerable<CardModel> CollectAllStatusCards()
    {
        foreach (var pileType in new[] { PileType.Hand, PileType.Discard, PileType.Draw })
        {
            foreach (var card in pileType.GetPile(Owner).Cards)
            {
                if (card.Type == CardType.Status)
                    yield return card;
            }
        }
    }

    private Creature? PickRandomEnemy()
    {
        var enemies = CombatState?.HittableEnemies.ToList();
        if (enemies == null || enemies.Count == 0)
            return null;

        return Owner.RunState.Rng.CombatTargets.NextItem(enemies);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
    }
}
