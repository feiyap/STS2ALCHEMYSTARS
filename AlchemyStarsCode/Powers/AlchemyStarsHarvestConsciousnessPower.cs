using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 收割意识：前 10 层每层 10% 伤害穿透格挡；超过 10 层每层 +10% 伤害；无格挡敌人每层多受 10% 伤害。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsHarvestConsciousnessPower : ModPowerTemplate
{
    private readonly Dictionary<Creature, (decimal OriginalBlock, decimal Amount, decimal PierceRatio)> _pierceAdjustments = new();

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (dealer != Owner || !props.IsPoweredAttack() || Amount <= 0 || target == null)
            return 1m;

        var mult = 1m;

        // 无格挡：每层额外 +10% 伤害。
        if (target.Block <= 0)
            mult *= 1m + Amount * 0.10m;

        // 超过 10 层：每超出 1 层 +10% 伤害。
        if (Amount > 10)
            mult *= 1m + (Amount - 10m) * 0.10m;

        return mult;
    }

    public override async Task BeforeDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (dealer != Owner || Amount <= 0)
            return;

        if (!props.IsPoweredAttack() || props.HasFlag(ValueProp.Unblockable))
            return;

        var pierceStacks = (int)decimal.Min(Amount, 10m);
        if (pierceStacks <= 0 || target.Block <= 0 || amount <= 0m)
            return;

        var pierceRatio = pierceStacks * 0.1m;
        var maxBlockable = amount * (1m - pierceRatio);
        var originalBlock = target.Block;

        if (originalBlock > maxBlockable)
            await CreatureCmd.LoseBlock(choiceContext, target, originalBlock - maxBlockable, null);

        _pierceAdjustments[target] = (originalBlock, amount, pierceRatio);
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (!_pierceAdjustments.Remove(target, out var adj))
            return;

        var wantBlock = decimal.Max(0m, adj.OriginalBlock - adj.Amount * (1m - adj.PierceRatio));
        var current = target.Block;
        if (current < wantBlock)
        {
            await CreatureCmd.GainBlock(
                target,
                new BlockVar(wantBlock - current, ValueProp.Unpowered),
                null);
        }
        else if (current > wantBlock)
        {
            await CreatureCmd.LoseBlock(choiceContext, target, current - wantBlock, null);
        }
    }
}
