using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
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
/// 反叛灼燃·莱因哈特：先古火/雷攻击。只能支付等同于耗能的生命打出；清空属性格后，若仅有火与雷则在回合结束时造成已损失生命 70% 伤害。
/// </summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class AlchemyStarsGeneratedRebellionBurningReinhardt : ModCardTemplate
{
    private const int BaseEnergyCost = 10;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Ancient;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = false;
    private const decimal BaseDamage = 10m;

    private decimal _hpPayCost;

    public override bool CanBeGeneratedInCombat => false;

    public override CardPoolModel VisualCardPool => ModelDb.CardPool<AlchemyStarsCardPool>();

    protected override bool IsPlayable => Owner.Creature.CurrentHp > HpPlayCost;

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseDamage, ValueProp.Move),
        AlchemyStarsKeywordText.InlineTitleVar("ThunderTitle", AlchemyStarsKeywordIds.Thunder),
        AlchemyStarsKeywordText.InlineTitleVar("FireTitle", AlchemyStarsKeywordIds.Fire)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Ethereal,
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.RebellionBurning)
    ];

    protected override HashSet<CardTag> CanonicalTags => [AlchemyStarsCardTags.RebellionBurning];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Ethereal),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Thunder)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.RebellionBurning)),
        HoverTipFactory.FromPower<AlchemyStarsRebellionBurningEchoPower>()
    ];

    private decimal HpPlayCost => EnergyCost.GetWithModifiers(CostModifiers.None);

    public AlchemyStarsGeneratedRebellionBurningReinhardt()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    public override bool TryModifyEnergyCostInCombatLate(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (!ReferenceEquals(card, this) || originalCost <= 0m)
            return false;

        if (Owner.Creature.CurrentHp <= originalCost)
            return false;

        _hpPayCost = originalCost;
        modifiedCost = 0m;
        return true;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var hpCost = _hpPayCost > 0m ? _hpPayCost : HpPlayCost;
        _hpPayCost = 0m;
        if (hpCost > 0m)
        {
            await CreatureCmd.Damage(
                choiceContext,
                Owner.Creature,
                hpCost,
                ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
                this,
                cardPlay);
        }

        if (LightMechanic.TryExhaustAllAttributeCellsOnlyThunderAndFire(Owner))
        {
            await PowerCmd.Apply<AlchemyStarsRebellionBurningEchoPower>(
                choiceContext,
                Owner.Creature,
                1m,
                Owner.Creature,
                this);
        }

        await LightMechanic.DealFireAndThunderAttackDamage(
            choiceContext,
            Owner,
            this,
            cardPlay.Target,
            DynamicVars.Damage.BaseValue,
            cardPlay);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-2);
        DynamicVars.Damage.UpgradeValueBy(-2m);
    }
}
