using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Events;
using STS2RitsuLib.Patching.Models;

namespace AlchemyStars.Patches;

/// <summary>
/// 原版事件立绘框固定为 2560×1200，并再放大 4%。在 16:9 画面里左右会超出屏幕。
/// 模组事件图按这个尺寸绘制，这里改为铺进可见区域并保持比例，让整张图都留在画面内。
/// </summary>
public sealed class EventPortraitFitPatch : IPatchMethod
{
    public static string PatchId => "alchemy_stars_event_portrait_fit";

    public static string Description =>
        "Fit Alchemy Stars event portraits inside the screen instead of the oversized 2560x1200 frame";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NEventLayout), nameof(NEventLayout.SetEvent), [typeof(EventModel)]),
    ];

    public static void Postfix(NEventLayout __instance, EventModel eventModel)
    {
        if (eventModel.GetType().Namespace != "AlchemyStars.Events")
            return;

        var portrait = __instance.GetNodeOrNull<TextureRect>("%Portrait");
        if (portrait == null || !GodotObject.IsInstanceValid(portrait))
            return;

        portrait.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        portrait.GrowHorizontal = Control.GrowDirection.Both;
        portrait.GrowVertical = Control.GrowDirection.Both;
        portrait.Scale = Vector2.One;
        portrait.PivotOffset = Vector2.Zero;
        portrait.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        portrait.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
    }
}
