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
using AlchemyStars.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 薇丝·空瞳：先古攻击。对所有敌人附加无时之印，按层数施加锁定并造成水属性伤害。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsViceEmptyPupil : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Ancient;
    private const TargetType CardTarget = TargetType.AllEnemies;
    private const bool ShowInCardLibrary = true;
    private const int SealStacks = 5;
    private const int SealStacksUpgradeBy = 2;
    private const decimal HitDamage = 2m;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(HitDamage, ValueProp.Move),
        new PowerVar<AlchemyStarsTimelessSealPower>(SealStacks),
        AlchemyStarsKeywordText.InlineTitleVar("WaterTitle", AlchemyStarsKeywordIds.Water),
        AlchemyStarsKeywordText.InlineTitleVar("LockTitle", AlchemyStarsKeywordIds.Lock),
        AlchemyStarsKeywordText.InlineTitleVar("TimelessSealTitle", AlchemyStarsKeywordIds.TimelessSeal)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Water)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.TimelessSeal)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Lock))
    ];

    public AlchemyStarsViceEmptyPupil()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var enemies = CombatState?.HittableEnemies.ToList();
        if (enemies == null || enemies.Count == 0)
            return;

        var hitDamage = DynamicVars.Damage.BaseValue;

        foreach (var enemy in enemies)
        {
            if (enemy.IsDead)
                continue;

            await PowerCmd.Apply<AlchemyStarsTimelessSealPower>(
                choiceContext,
                enemy,
                DynamicVars["AlchemyStarsTimelessSealPower"].BaseValue,
                Owner.Creature,
                this);

            var sealAmount = enemy.GetPowerAmount<AlchemyStarsTimelessSealPower>();
            if (sealAmount <= 0)
                continue;

            await PowerCmd.Apply<AlchemyStarsLockPower>(
                choiceContext,
                enemy,
                sealAmount,
                Owner.Creature,
                this);

            for (var i = 0; i < sealAmount; i++)
            {
                if (enemy.IsDead)
                    break;

                await LightMechanic.DealElementalAttackDamage(
                    choiceContext,
                    Owner,
                    this,
                    enemy,
                    hitDamage,
                    LightElement.Water,
                    cardPlay);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["AlchemyStarsTimelessSealPower"].UpgradeValueBy(SealStacksUpgradeBy);
    }
}
