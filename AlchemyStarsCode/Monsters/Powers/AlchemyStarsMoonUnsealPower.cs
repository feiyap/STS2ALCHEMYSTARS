using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Monsters.Powers;

/// <summary>
/// 解封：月光。Amount 为需要承受的攻击次数，图标显示剩余次数。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsMoonUnsealPower : AlchemyStarsSwordAltarPowerBase
{
    public int HitsTaken { get; private set; }

    protected override string PlaceholderIcon => "AlchemyStarsMoonUnsealPower";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override int DisplayAmount => System.Math.Max(0, (int)Amount - HitsTaken);

    public void RegisterHit()
    {
        AssertMutable();
        HitsTaken++;
        InvokeDisplayAmountChanged();
    }
}
