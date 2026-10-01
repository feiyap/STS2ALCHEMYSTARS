using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Monsters.Powers;

/// <summary>
/// 绝念：二阶段获得格挡时，额外加上当前力量。具体加值由祭剑座结算。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsAbsoluteResolvePower : AlchemyStarsSwordAltarPowerBase
{
    protected override string PlaceholderIcon => "AlchemyStarsAbsoluteResolvePower";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override int DisplayAmount => 0;
}
