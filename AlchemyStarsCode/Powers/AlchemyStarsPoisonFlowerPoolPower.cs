using System.Collections.Generic;
using AlchemyStars.Mechanics;
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 花海毒池：此牌消耗光能生成的属性格必然为深色格�?/// </summary>
[RegisterPower]
public sealed class AlchemyStarsPoisonFlowerPoolPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<string> RegisteredKeywordIds => ["poison_flower_pool"];

    public static AttributeCellKind ResolveSpawnKind(AttributeCellKind defaultKind) =>
        defaultKind == AttributeCellKind.Normal ? AttributeCellKind.Dark : defaultKind;
}
