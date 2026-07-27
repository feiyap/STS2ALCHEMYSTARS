using SmartFormat.Core.Extensions;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Localization;

/// <summary>
/// 为本地化提供光能属性前缀占位符，供 <see cref="LightIconsFormatter"/> 使用。
/// </summary>
[RegisterSmartFormatSource]
public sealed class LightIconPrefixSource : ISource
{
    private static readonly Dictionary<string, string> PrefixBySelector =
        new(StringComparer.Ordinal)
        {
            ["forestLightPrefix"] = LightIconPaths.Forest,
            ["waterLightPrefix"] = LightIconPaths.Water,
            ["fireLightPrefix"] = LightIconPaths.Fire,
            ["thunderLightPrefix"] = LightIconPaths.Thunder,
        };

    public bool TryEvaluateSelector(ISelectorInfo selectorInfo)
    {
        // 仅解析占位符首段，避免误伤属性链（如 obj.forestLightPrefix）
        if (selectorInfo.SelectorIndex != 0)
            return false;

        if (!PrefixBySelector.TryGetValue(selectorInfo.SelectorText, out var prefix))
            return false;

        selectorInfo.Result = prefix;
        return true;
    }
}
