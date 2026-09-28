using AlchemyStars.Mechanics;
using Godot;

namespace AlchemyStars.UI;

/// <summary>
/// 光能 / 转色格战斗 UI 贴图路径与加载。
/// </summary>
internal static class LightMechanicUiAssets
{
    private const string LightDir = $"{Entry.ResPath}/images/ui/light";
    private const string CellDir = $"{Entry.ResPath}/images/ui/cells";

    private static readonly Dictionary<string, Texture2D?> Cache = new(StringComparer.Ordinal);

    public static string GetLightIconPath(LightElement element) => element switch
    {
        LightElement.Forest => $"{LightDir}/forest_light_icon.png",
        LightElement.Thunder => $"{LightDir}/thunder_light_icon.png",
        LightElement.Water => $"{LightDir}/water_light_icon.png",
        LightElement.Fire => $"{LightDir}/fire_light_icon.png",
        // 万色暂无无后缀资源，继续使用 _ui 贴图。
        LightElement.Prismatic => $"{LightDir}/prismatic_light_icon_ui.png",
        _ => $"{LightDir}/forest_light_icon.png",
    };

    public static string GetCellTexturePath(LightElement element) => element switch
    {
        LightElement.Forest => $"{CellDir}/forest_cell.png",
        LightElement.Thunder => $"{CellDir}/thunder_cell.png",
        LightElement.Water => $"{CellDir}/water_cell.png",
        LightElement.Fire => $"{CellDir}/fire_cell.png",
        LightElement.Prismatic => $"{CellDir}/prismatic_cell.png",
        _ => $"{CellDir}/forest_cell.png",
    };

    private const string AttributeDir = $"{Entry.ResPath}/images/ui/attributes";

    /// <summary>卡面费用下方的属性角标贴图。</summary>
    public static string GetCardAttributeIconPath(LightElement element) => element switch
    {
        LightElement.Forest => $"{AttributeDir}/forest_attribute_icon.png",
        LightElement.Thunder => $"{AttributeDir}/thunder_attribute_icon.png",
        LightElement.Water => $"{AttributeDir}/water_attribute_icon.png",
        LightElement.Fire => $"{AttributeDir}/fire_attribute_icon.png",
        _ => $"{AttributeDir}/forest_attribute_icon.png",
    };

    /// <summary>属性格出现时的光点贴图。</summary>
    public static string CellSparkPath => $"{CellDir}/cell_spark.png";

    /// <summary>属性格出现时的光线贴图。</summary>
    public static string CellRayPath => $"{CellDir}/cell_ray.png";

    public static Texture2D? Load(string path)
    {
        if (Cache.TryGetValue(path, out var cached))
            return cached;

        var texture = ResourceLoader.Load<Texture2D>(path);
        Cache[path] = texture;
        return texture;
    }
}
