using AlchemyStars.Relics.Ancients;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Patching.Models;

namespace AlchemyStars.Patches;

/// <summary>
/// 顺心罗盘：进入精英房时使用地图坐标预分配的遭遇。
/// </summary>
public sealed class SmoothCompassEliteEncounterPatch : IPatchMethod
{
    public static string PatchId => "alchemy_stars_smooth_compass_elite_encounter";
    public static string Description => "Pull compass-assigned elite encounters by map coord";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(ActModel), nameof(ActModel.PullNextEncounter)),
    ];

    public static bool Prefix(ActModel __instance, RoomType roomType, ref EncounterModel __result)
    {
        if (roomType != RoomType.Elite)
            return true;

        var runState = RunManager.Instance?.State;
        if (runState == null || runState.Act != __instance)
            return true;

        var compass = AlchemyStarsSmoothCompass.FindActive(runState);
        if (compass == null)
            return true;

        if (!runState.CurrentMapCoord.HasValue)
            return true;

        if (!compass.TryGetEliteEncounter(runState, runState.CurrentMapCoord.Value, out var encounter)
            || encounter == null)
            return true;

        __result = encounter;
        return false;
    }
}
