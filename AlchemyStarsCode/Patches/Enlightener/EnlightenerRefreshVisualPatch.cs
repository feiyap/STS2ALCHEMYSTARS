using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib.Patching.Models;

namespace AlchemyStars.Patches.Enlightener;

/// <summary>
/// 事件状态刷新时套用启迪者标题。
/// </summary>
public sealed class EnlightenerRefreshVisualPatch : IPatchMethod
{
    public static string PatchId => "alchemy_stars_enlightener_refresh_visual";

    public static string Description => "Apply Enlightener title when follow-up page is active";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NEventRoom), "RefreshEventState", [typeof(EventModel)]),
    ];

    public static void Postfix(EventModel eventModel)
    {
        if (EnlightenerFollowUpVisualState.TryGet(eventModel, out var eventEntry))
            EnlightenerFollowUpVisuals.Apply(eventEntry);
    }
}
