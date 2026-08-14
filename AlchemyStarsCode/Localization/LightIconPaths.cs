namespace AlchemyStars.Localization;

/// <summary>
/// 光能描述小图标路径与属性名解析。
/// </summary>
public static class LightIconPaths
{
    public const string Forest = "forest";
    public const string Water = "water";
    public const string Fire = "fire";
    public const string Thunder = "thunder";

    private static readonly HashSet<string> ValidElements =
        new(StringComparer.OrdinalIgnoreCase)
        {
            Forest,
            Water,
            Fire,
            Thunder,
        };

    private static readonly Dictionary<string, string> IconFileByElement =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [Forest] = "forest_light_icon_ui.png",
            [Water] = "water_light_icon_ui.png",
            [Fire] = "fire_light_icon_ui.png",
            [Thunder] = "thunder_light_icon_ui.png",
        };

    public static bool IsValidElement(string? element) =>
        !string.IsNullOrWhiteSpace(element) && ValidElements.Contains(element.Trim());

    public static string NormalizeElement(string element) =>
        element.Trim().ToLowerInvariant();

    public static bool TryGetIconTag(string element, out string iconTag)
    {
        iconTag = string.Empty;
        if (!IconFileByElement.TryGetValue(NormalizeElement(element), out var fileName))
            return false;

        iconTag = $"[img]{Entry.ResPath}/images/ui/light/{fileName}[/img]";
        return true;
    }
}
