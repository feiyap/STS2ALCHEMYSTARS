using System.Linq;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SmartFormat.Core.Extensions;
using STS2RitsuLib.Interop.AutoRegistration;

namespace AlchemyStars.Localization;

/// <summary>
/// 将数量渲染为属性光能小图标，用法对齐原版 <c>energyIcons</c>。
/// </summary>
/// <remarks>
/// 固定数量：<c>{forestLightPrefix:lightIcons(1)}</c><br/>
/// 动态变量：<c>{WaterGain:lightIcons(water)}</c>
/// </remarks>
[RegisterSmartFormatter]
public sealed class LightIconsFormatter : IFormatter
{
    public string Name
    {
        get => "lightIcons";
        set => throw new NotImplementedException();
    }

    public bool CanAutoDetect { get; set; }

    public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
    {
        if (!TryResolve(formattingInfo, out var element, out var amount, out var dynamicVar))
            return false;

        if (!LightIconPaths.TryGetIconTag(element, out var iconTag))
            throw new LocException($"Unknown light energy element='{element}'");

        var text = amount is > 0 and < 4
            ? string.Concat(Enumerable.Repeat(iconTag, amount))
            : dynamicVar == null
                ? $"{amount}{iconTag}"
                : dynamicVar.ToHighlightedString(inverse: false) + iconTag;

        formattingInfo.Write(text);
        return true;
    }

    private static bool TryResolve(
        IFormattingInfo formattingInfo,
        out string element,
        out int amount,
        out DynamicVar? dynamicVar)
    {
        element = string.Empty;
        amount = 0;
        dynamicVar = null;

        var options = formattingInfo.FormatterOptions?.Trim() ?? string.Empty;
        switch (formattingInfo.CurrentValue)
        {
            case DynamicVar value:
                if (string.IsNullOrWhiteSpace(options) || !LightIconPaths.IsValidElement(options))
                    return false;

                element = LightIconPaths.NormalizeElement(options);
                amount = Convert.ToInt32(value.PreviewValue);
                dynamicVar = value;
                return true;

            case string prefix:
                if (!int.TryParse(options, out amount))
                    return false;

                element = LightIconPaths.NormalizeElement(prefix);
                return LightIconPaths.IsValidElement(element);

            case int value:
                if (string.IsNullOrWhiteSpace(options) || !LightIconPaths.IsValidElement(options))
                    return false;

                element = LightIconPaths.NormalizeElement(options);
                amount = value;
                return true;

            case decimal value:
                if (string.IsNullOrWhiteSpace(options) || !LightIconPaths.IsValidElement(options))
                    return false;

                element = LightIconPaths.NormalizeElement(options);
                amount = (int)value;
                return true;

            default:
                throw new LocException(
                    $"Unknown value='{formattingInfo.CurrentValue}' type={formattingInfo.CurrentValue?.GetType()}");
        }
    }
}
