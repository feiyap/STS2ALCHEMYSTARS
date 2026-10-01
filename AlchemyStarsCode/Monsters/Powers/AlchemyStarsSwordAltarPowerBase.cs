using AlchemyStars.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace AlchemyStars.Monsters.Powers;

/// <summary>
/// 祭剑座能力基类。图标使用导入的祭剑座能力图。
/// </summary>
public abstract class AlchemyStarsSwordAltarPowerBase : AlchemyStarsPowerBase
{
    /// <summary>已有能力图的文件名（不含扩展名）。</summary>
    protected abstract string PlaceholderIcon { get; }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{PlaceholderIcon}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{PlaceholderIcon}.png");
}
