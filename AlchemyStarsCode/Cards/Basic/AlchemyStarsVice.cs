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
using AlchemyStars.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

[RegisterCard(typeof(AlchemyStarsCardPool))]
[RegisterCharacterStarterCard(typeof(AlchemyStarsCharacter), 1)]
[RegisterArchaicToothTranscendence(typeof(AlchemyStarsViceEmptyPupil))]
public sealed class AlchemyStarsVice : ModCardTemplate
{
    private const string CalculatedHitsKey = "CalculatedHits";
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Basic;
    private const TargetType CardTarget = TargetType.RandomEnemy;
    private const bool ShowInCardLibrary = true;
    private const int BaseHitCount = 5;
    private const int BonusHitCount = 1;
    private const int MaxWaterLightConsume = 2;
    private const decimal HitDamage = 1m;
    private const decimal LightDamageBonus = 1m;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(HitDamage, ValueProp.Move),
        new CalculationBaseVar(BaseHitCount),
        new CalculationExtraVar(BonusHitCount),
        new CalculatedVar(CalculatedHitsKey).WithMultiplier(CountLightBonusMultiplier),
        AlchemyStarsKeywordText.InlineTitleVar("WaterTitle", AlchemyStarsKeywordIds.Water),
        AlchemyStarsKeywordText.InlineTitleVar("LockTitle", AlchemyStarsKeywordIds.Lock)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Lock))
    ];

    public AlchemyStarsVice() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var consumed = 0;
        for (var n = 0; n < MaxWaterLightConsume; n++)
        {
            if (!LightMechanic.TryConsumeLightEnergy(Owner, [LightElement.Water]))
                break;

            consumed++;
        }

        var hitCount = DynamicVars.CalculationBase.IntValue;
        if (consumed >= 2)
            hitCount += DynamicVars.CalculationExtra.IntValue;

        var hitDamage = DynamicVars.Damage.BaseValue;
        if (consumed >= 1)
            hitDamage += LightDamageBonus;

        for (var i = 0; i < hitCount; i++)
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
                LightElement.Water,
                cardPlay);

            if (IsUpgraded)
            {
                await PowerCmd.Apply<AlchemyStarsLockPower>(
                    choiceContext,
                    target,
                    1,
                    Owner.Creature,
                    this);
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

    private static decimal CountLightBonusMultiplier(CardModel card, Creature? _)
    {
        if (card.Owner == null)
            return 0m;

        return LightMechanic.CountWaterLightEnergy(card.Owner) >= MaxWaterLightConsume ? 1m : 0m;
    }
}
