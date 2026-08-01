using System.Collections.Generic;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Entities.Powers;
using AlchemyStars.Keywords;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 觉醒形态：每消耗 15 点光能，将转色栏重置为四种不同属性。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsAwakeningFormPower : ModPowerTemplate
{
    private const int LightEnergyPerReset = 15;

    private int _pendingConsumed;
    private bool _allowSpecialCells;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<string> RegisteredKeywordIds => [AlchemyStarsKeywordIds.AwakeningForm];

    /// <summary>升级后重置时有概率生成深色格与强化格。</summary>
    public void Configure(bool allowSpecialCells) => _allowSpecialCells = allowSpecialCells;

    internal void NotifyLightEnergyConsumed(int count)
    {
        if (count <= 0)
            return;

        var player = Owner.Player;
        if (player == null)
            return;

        _pendingConsumed += count;
        while (_pendingConsumed >= LightEnergyPerReset)
        {
            _pendingConsumed -= LightEnergyPerReset;
            Flash();
            LightMechanic.ResetAttributeBarWithFourDistinct(player, _allowSpecialCells);
        }
    }
}
