using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Monsters.Powers;

/// <summary>
/// 晦暗皇帝：格挡被打破时，由祭剑座把下一动改为眩晕。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsDarkEmperorPower : AlchemyStarsSwordAltarPowerBase
{
    protected override string PlaceholderIcon => "AlchemyStarsDarkestRoarPower";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override int DisplayAmount => 0;
}
