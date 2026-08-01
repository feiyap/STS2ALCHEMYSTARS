using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Powers;
using AlchemyStars.Keywords;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 帝国雷霆：正道威严等效果的目标标记�?/// </summary>
[RegisterPower]
public sealed class AlchemyStarsImperialThunderPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<string> RegisteredKeywordIds => [AlchemyStarsKeywordIds.ImperialThunder];
}
