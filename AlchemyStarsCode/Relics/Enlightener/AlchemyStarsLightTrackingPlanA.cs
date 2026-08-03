using AlchemyStars.Characters;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Relics.Enlightener;

/// <summary>
/// 光能追踪方案 A：锁定属性后，商店与奖励中的属性卡仅出现该属性。
/// </summary>
[RegisterRelic(typeof(AlchemyStarsRelicPool))]
public sealed class AlchemyStarsLightTrackingPlanA : AlchemyStarsLightTrackingLockRelicBase
{
    protected override bool AffectsShop => true;
}
