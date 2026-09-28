using System.Threading.Tasks;
using AlchemyStars.Relics.Ancients;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using STS2RitsuLib.Patching.Models;

namespace AlchemyStars.Patches;

/// <summary>
/// 离开事件房时，若持有多门钥匙且存在 Pending 事件，则进入该回忆事件。
/// </summary>
public sealed class KeyOfManyDoorsPendingEventPatch : IPatchMethod
{
    public static string PatchId => "alchemy_stars_key_of_many_doors_pending_event";

    public static string Description => "Enter pending memory event after leaving event/ancient room";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(EventRoom), nameof(EventRoom.Exit)),
    ];

    public static void Postfix(EventRoom __instance, IRunState? runState, ref Task __result)
    {
        __result = ContinueAfterExit(__instance, runState, __result);
    }

    private static async Task ContinueAfterExit(EventRoom room, IRunState? runState, Task original)
    {
        await original;

        if (runState == null || RunManager.Instance == null)
            return;

        foreach (var player in runState.Players)
        {
            var key = player.GetRelic<AlchemyStarsKeyOfManyDoors>();
            if (key?.PendingEventEntry == null)
                continue;

            // 若刚离开的正是待进入事件本身，仅清空标记，避免循环。
            if (room.CanonicalEvent.Id == key.PendingEventEntry)
            {
                key.PendingEventEntry = null;
                continue;
            }

            var pendingId = key.PendingEventEntry;
            key.PendingEventEntry = null;

            var eventModel = SaveUtil.EventOrDeprecated(pendingId);
            await RunManager.Instance.EnterRoom(new EventRoom(eventModel));
            break;
        }
    }
}
