using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Powers;

/// <summary>
/// 本 Mod 能力基类：默认使用 <c>images/powers/{类型名}.png</c> 作为图标。
/// </summary>
public abstract class AlchemyStarsPowerBase : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");
}
