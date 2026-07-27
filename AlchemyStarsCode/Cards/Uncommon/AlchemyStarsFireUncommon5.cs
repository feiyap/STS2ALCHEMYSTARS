using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
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
/// 蜜雅·涤魂：芭芭雅嘎茧生；损血转火伤加成覆甲、获能并火攻，斩杀时覆甲转生命。
/// </summary>
[RegisterCard(typeof(AlchemyStarsCardPool))]
public sealed class AlchemyStarsFireUncommon5 : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;
    private const decimal HpLossPercent = 0.07m;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1),
        new DamageVar(15m, ValueProp.Move),
        AlchemyStarsKeywordText.InlineTitleVar("BabaYagaCocoon", AlchemyStarsKeywordIds.BabaYagaCocoon),
        AlchemyStarsKeywordText.InlineTitleVar("FireTitle", AlchemyStarsKeywordIds.Fire)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.BabaYagaCocoon)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.BabaYagaCocoon)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.LightEnergy)),
        HoverTipFactory.FromPower<PlatingPower>()
    ];

    public AlchemyStarsFireUncommon5()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var target = cardPlay.Target;

        var lostHp = (int)System.Math.Ceiling(Owner.Creature.CurrentHp * HpLossPercent);
        if (lostHp > 0)
        {
            await CreatureCmd.Damage(
                choiceContext,
                Owner.Creature,
                lostHp,
                ValueProp.Unblockable | ValueProp.Unpowered,
                null,
                null);

            var fireMultiplier = LightMechanic.GetOutgoingDamageMultiplier(Owner, LightElement.Fire);
            var plating = (int)System.Math.Ceiling(lostHp * fireMultiplier);
            if (plating > 0)
            {
                await PowerCmd.Apply<PlatingPower>(
                    choiceContext,
                    Owner.Creature,
                    plating,
                    Owner.Creature,
                    this);
            }
        }

        await PlayerCmd.GainEnergy(1, Owner);

        await LightMechanic.DealElementalAttackDamage(
            choiceContext,
            Owner,
            this,
            target,
            DynamicVars.Damage.BaseValue,
            LightElement.Fire,
            cardPlay);

        if (target.IsDead)
            await ConvertPlatingToHeal(choiceContext);
    }

    private async Task ConvertPlatingToHeal(PlayerChoiceContext choiceContext)
    {
        var plating = Owner.Creature.GetPowerAmount<PlatingPower>();
        if (plating <= 0)
            return;

        await CreatureCmd.Heal(Owner.Creature, plating);
        await PowerCmd.Remove<PlatingPower>(Owner.Creature);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
