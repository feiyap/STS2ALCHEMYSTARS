using AlchemyStars.Characters;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Enlightener;

/// <summary>
/// 光能追踪方案 B：锁定属性后，仅奖励中的属性卡限定为该属性。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsLightTrackingPlanB : AlchemyStarsLightTrackingLockRelicBase
{
    protected override bool AffectsShop => false;
}
