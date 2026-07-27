using System;
using System.Threading.Tasks;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 光之剑甲：以层数代表剩余血量，耗尽时为召唤者生�?1 个森属性强化格�?/// </summary>
[RegisterPower]
public sealed class AlchemyStarsLightSwordArmorPower : ModPowerTemplate
{
    private sealed class Data
    {
        public decimal DamageAbsorbed;
    }

    private Player? _summoner;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override object InitInternalData() => new Data();

    internal void Configure(Player summoner) => _summoner = summoner;

    public override decimal ModifyHpLostBeforeOstyLate(
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner || amount <= 0m || Amount <= 0m)
            return amount;

        var remaining = Amount - GetInternalData<Data>().DamageAbsorbed;
        if (remaining <= 0m)
            return amount;

        return Math.Max(0m, amount - remaining);
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner || result.WasFullyBlocked)
            return;

        var data = GetInternalData<Data>();
        data.DamageAbsorbed += result.UnblockedDamage;
        Flash();

        if (data.DamageAbsorbed < Amount)
            return;

        var summoner = _summoner;
        if (summoner != null && LightMechanic.HasMechanicRelic(summoner))
        {
            LightMechanic.TryAddAttributeCell(
                summoner,
                LightElement.Forest,
                AttributeCellKind.Enhanced);
        }

        await PowerCmd.Remove(this);
    }
}
