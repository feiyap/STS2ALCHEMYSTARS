using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
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
/// 维多利亚·墓歌：X 费先古火攻击；伤害 = 倍率 × X × 火格，宽恕的全知与血月狂宴增幅，未格挡伤生成火深色格。
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
        // 伤害 = (1/2) × 消耗费用 × 火属性格子数量。
        var damage = DynamicVars.Damage.BaseValue * x * fireCells;

        // 宽恕的全知：按目标已损失生命百分比增幅。
        var target = cardPlay.Target;
        if (target.MaxHp > 0m)
        {
            var lostRatio = (target.MaxHp - target.CurrentHp) / target.MaxHp;
            if (lostRatio > 0m)
                damage *= 1m + lostRatio;
        }

        // 血月狂宴：消耗全部灼燃，每层 +20%。
        var ignition = Owner.Creature.GetPower<AlchemyStarsIgnitionPower>();
        var ignitionStacks = ignition?.Amount ?? 0m;
        if (ignitionStacks > 0m && ignition != null)
        {
            damage *= 1m + IgnitionBonusPerStack * ignitionStacks;
            await PowerCmd.Remove(ignition);
        }

        // 升级：拥有 4 个火深色格时最终伤害翻倍。
        if (IsUpgraded && LightMechanic.CountFireDarkCells(Owner) >= UpgradeDarkCellDoubleThreshold)
            damage *= 2m;

        if (damage <= 0m)
            return;

        decimal unblocked;
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

        if (unblocked > 0m)
        {
            for (var i = 0; i < DarkCellsOnUnblocked; i++)
                LightMechanic.TryAddAttributeCell(Owner, LightElement.Fire, AttributeCellKind.Dark);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(DamageMultiplierUpgradeBy);
    }
}
