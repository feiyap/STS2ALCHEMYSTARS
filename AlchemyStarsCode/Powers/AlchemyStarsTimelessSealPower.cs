using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Powers;
using AlchemyStars.Keywords;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 无时之印：可叠加的标记，供薇丝·空瞳等效果读取层数。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsTimelessSealPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<string> RegisteredKeywordIds => [AlchemyStarsKeywordIds.TimelessSeal];
}
