using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 旧印威能：只给旧印卡消耗。被消耗时减层，层数耗尽后消失。不提供力量。
/// </summary>
[RegisterPower]
public sealed class AlchemyStarsOldSealMightPower : AlchemyStarsPowerBase
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/AlchemyStarsOldSealMight.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/AlchemyStarsOldSealMight.png");

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
}
