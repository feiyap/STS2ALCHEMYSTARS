using AlchemyStars.Relics.Ancients;
using Godot;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using STS2RitsuLib.Patching.Models;

namespace AlchemyStars.Patches;

/// <summary>
/// 顺心罗盘：在精英/Boss 地图节点旁添加遭遇名文本。
/// </summary>
public sealed class SmoothCompassBossLabelPatch : IPatchMethod
{
    public static string PatchId => "alchemy_stars_smooth_compass_boss_label";
    public static string Description => "Show boss encounter name beside boss map points";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NBossMapPoint), nameof(NBossMapPoint._Ready)),
    ];

    public static void Postfix(NBossMapPoint __instance)
    {
        SmoothCompassMapLabelHelper.TryAttachLabel(__instance);
    }
}

/// <summary>
/// 顺心罗盘：精英节点标签。
/// </summary>
public sealed class SmoothCompassEliteLabelPatch : IPatchMethod
{
    public static string PatchId => "alchemy_stars_smooth_compass_elite_label";
    public static string Description => "Show elite encounter name beside elite map points";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NNormalMapPoint), nameof(NNormalMapPoint._Ready)),
    ];

    public static void Postfix(NNormalMapPoint __instance)
    {
        if (__instance.Point.PointType != MapPointType.Elite)
            return;

        SmoothCompassMapLabelHelper.TryAttachLabel(__instance);
    }
}

internal static class SmoothCompassMapLabelHelper
{
    private const string LabelName = "AlchemyStarsSmoothCompassLabel";

    public static void TryAttachLabel(NMapPoint mapPoint)
    {
        var runState = MegaCrit.Sts2.Core.Runs.RunManager.Instance?.State;
        if (runState == null)
            return;

        var compass = AlchemyStarsSmoothCompass.FindActive(runState);
        if (compass == null)
            return;

        var text = compass.GetLabelForPoint(runState, mapPoint.Point);
        if (string.IsNullOrWhiteSpace(text))
            return;

        if (mapPoint.GetNodeOrNull(LabelName) is Label existing)
        {
            existing.Text = text;
            return;
        }

        var label = new Label
        {
            Name = LabelName,
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Position = new Vector2(-64f, 36f),
            Size = new Vector2(128f, 48f),
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
        };
        label.AddThemeColorOverride("font_color", Colors.White);
        label.AddThemeColorOverride("font_shadow_color", Colors.Black);
        label.AddThemeConstantOverride("shadow_offset_x", 1);
        label.AddThemeConstantOverride("shadow_offset_y", 1);
        label.AddThemeFontSizeOverride("font_size", 14);
        mapPoint.AddChild(label);
    }
}
