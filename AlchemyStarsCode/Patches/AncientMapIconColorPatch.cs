using System.Reflection;
using AlchemyStars.Events;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Patching.Models;

namespace AlchemyStars.Patches;

/// <summary>
/// 泽塔与威廉的地图图标是全彩立绘。
/// 可到达、已走过和悬停时，原版会把图标乘上近黑的 <c>pathDotTraveled</c>，这里改成白色以保留原色。
/// </summary>
public sealed class AncientMapIconColorPatch : IPatchMethod
{
    public static string PatchId => "alchemy_stars_ancient_map_icon_color";

    public static string Description =>
        "Keep Zeta and William map icons at full color instead of the near-black map tint";

    public static bool IsCritical => false;

    private static readonly FieldInfo? RunStateField =
        AccessTools.Field(typeof(NMapPoint), "_runState");

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NAncientMapPoint), "TraveledColor", MethodType.Getter),
        new(typeof(NAncientMapPoint), "HoveredColor", MethodType.Getter),
    ];

    public static void Postfix(NAncientMapPoint __instance, ref Color __result)
    {
        if (IsFullColorAncient(__instance))
            __result = Colors.White;
    }

    private static bool IsFullColorAncient(NAncientMapPoint point)
    {
        if (RunStateField?.GetValue(point) is not IRunState runState)
            return false;

        return runState.Act.Ancient is AlchemyStarsZeta or AlchemyStarsWilliam;
    }
}
