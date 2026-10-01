using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using AlchemyStars.Keywords;
using AlchemyStars.Mechanics;
using AlchemyStars.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Cards;

/// <summary>
/// 维多利亚·墓歌：X 费先古火攻击；基伤 1/升级 2 可吃附魔，再 × 费用 × 火格。
/// </summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class AlchemyStarsVictoriaGraveSong : ModCardTemplate
{
    private const int BaseEnergyCost = 0;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Ancient;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = false;
    private const decimal BaseDamageMultiplier = 1m;
    private const decimal DamageMultiplierUpgradeBy = 1m;
    private const decimal IgnitionBonusPerStack = 0.20m;
    private const int DarkCellsOnUnblocked = 2;
    private const int UpgradeDarkCellDoubleThreshold = 4;

    protected override bool HasEnergyCostX => true;

    public override bool CanBeGeneratedInCombat => false;

    public override bool CanBeGeneratedByModifiers => false;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/AlchemyStarsVictoriaGraveSong.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseDamageMultiplier, ValueProp.Move),
        AlchemyStarsKeywordText.InlineTitleVar("FireTitle", AlchemyStarsKeywordIds.Fire),
        AlchemyStarsKeywordText.InlineTitleVar("ForgivingOmniscience", AlchemyStarsKeywordIds.ForgivingOmniscience),
        AlchemyStarsKeywordText.InlineTitleVar("BloodMoonBanquet", AlchemyStarsKeywordIds.BloodMoonBanquet),
        AlchemyStarsKeywordText.InlineTitleVar("Ignition", AlchemyStarsKeywordIds.Ignition)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.ForgivingOmniscience),
        ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.BloodMoonBanquet)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Fire)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.ForgivingOmniscience)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.BloodMoonBanquet)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.Ignition)),
        HoverTipFactory.FromKeyword(ModKeywordRegistry.GetCardKeyword(AlchemyStarsKeywordIds.DarkCell)),
        HoverTipFactory.FromPower<AlchemyStarsIgnitionPower>()
    ];

    public AlchemyStarsVictoriaGraveSong()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var x = ResolveEnergyXValue();
        if (x <= 0)
            return;

        var fireCells = LightMechanic.CountFireAttributeCells(Owner);
        // 基伤（1/升级 2）先吃活力、锋利等附魔，再 × 费用 × 火格。
        var baseMul = ApplyEnchantmentToBase(DynamicVars.Damage.BaseValue, DynamicVars.Damage.Props);
        var damage = baseMul * x * fireCells;

        var target = cardPlay.Target;
        if (target.MaxHp > 0m)
        {
            var lostRatio = (target.MaxHp - target.CurrentHp) / target.MaxHp;
            if (lostRatio > 0m)
                damage *= 1m + lostRatio;
        }

        var ignition = Owner.Creature.GetPower<AlchemyStarsIgnitionPower>();
        var ignitionStacks = ignition?.Amount ?? 0m;
        if (ignitionStacks > 0m && ignition != null)
        {
            damage *= 1m + IgnitionBonusPerStack * ignitionStacks;
            await PowerCmd.Remove(ignition);
        }

        if (IsUpgraded && LightMechanic.CountFireDarkCells(Owner) >= UpgradeDarkCellDoubleThreshold)
            damage *= 2m;

        if (damage <= 0m)
            return;

        var unblocked = 0m;
        await WithEnchantmentDetached(async () =>
        {
            using (LightMechanicDamageContext.Use(LightElement.Fire))
            {
                var attack = DamageCmd.Attack(damage)
                    .FromCard(this, cardPlay)
                    .Targeting(target);
                await attack.Execute(choiceContext);
                unblocked = attack.Results
                    .SelectMany(result => result)
                    .Sum(result => (decimal)result.UnblockedDamage);
            }

            await LightMechanic.ApplyElementalHitEffects(
                choiceContext,
                Owner,
                target,
                LightElement.Fire,
                this);
        });

        if (unblocked > 0m)
        {
            for (var i = 0; i < DarkCellsOnUnblocked; i++)
                LightMechanic.TryAddAttributeCell(Owner, LightElement.Fire, AttributeCellKind.Dark);
        }
    }

    private decimal ApplyEnchantmentToBase(decimal baseDamage, ValueProp props)
    {
        var enchantment = Enchantment;
        if (enchantment == null)
            return baseDamage;

        var value = baseDamage;
        value += enchantment.EnchantDamageAdditive(value, props);
        value *= enchantment.EnchantDamageMultiplicative(value, props);
        return Math.Max(0m, value);
    }

    private async Task WithEnchantmentDetached(Func<Task> action)
    {
        var enchantment = Enchantment;
        if (enchantment == null)
        {
            await action();
            return;
        }

        var amount = enchantment.Amount;
        var clone = (EnchantmentModel)enchantment.MutableClone();
        ClearEnchantmentInternal();
        try
        {
            await action();
        }
        finally
        {
            EnchantInternal(clone, amount);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(DamageMultiplierUpgradeBy);
    }
}
