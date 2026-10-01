using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Monsters.Powers;

/// <summary>
/// 极暗咆哮：格挡被打破则跳过下一次攻击。判定由祭剑座在回合开始前读取剩余格挡。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsDarkestRoarPower : AlchemyStarsSwordAltarPowerBase
{
    protected override string PlaceholderIcon => "AlchemyStarsDarkestRoarPower";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override int DisplayAmount => 0;
}
