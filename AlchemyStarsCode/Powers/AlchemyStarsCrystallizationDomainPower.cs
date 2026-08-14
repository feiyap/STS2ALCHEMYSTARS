using System.Collections.Generic;
using AlchemyStars.Keywords;
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 结晶领域（遗留能力注册）：效果已改为由手牌中的结晶领域卡在 AfterCardPlayed 中触发。
/// 保留此类以免旧存档/引用缺失；不再由拜里厄打出时施加。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsCrystallizationDomainPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<string> RegisteredKeywordIds => [AlchemyStarsKeywordIds.Crystallization];
}
