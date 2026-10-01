using AlchemyStars.Relics.Ancients;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using STS2RitsuLib.Patching.Models;

namespace AlchemyStars.Patches;

/// <summary>
/// 先古事件点「继续」时：若万门之钥有待进入的回忆事件，则进入该事件而非打开地图。
/// <para>
/// 说明：原版 Proceed 只开地图、不调用 EventRoom.Exit；挂在 Exit 上会永远等不到，
/// 且在地图旅行的 EnterRoom 流程里嵌套 EnterRoom 会被目标房间覆盖。
/// </para>
/// </summary>
public sealed class KeyOfManyDoorsPendingEventPatch : IPatchMethod
{
    public static string PatchId => "alchemy_stars_key_of_many_doors_pending_event";

    public static string Description =>
        "Enter pending Key of Many Doors memory event on ancient Proceed instead of opening the map";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NEventRoom), nameof(NEventRoom.Proceed)),
    ];

    public static bool Prefix(ref Task __result)
    {
        var run = RunManager.Instance;
        var state = run?.State;
        if (run == null || state == null)
            return true;

        foreach (var player in state.Players)
        {
            var key = player.GetRelic<AlchemyStarsKeyOfManyDoors>();
            if (key?.PendingEventEntry == null)
                continue;

            var pendingId = key.PendingEventEntry;
            key.PendingEventEntry = null;

            Entry.Logger.Info($"[KeyOfManyDoors] Proceed → 进入回忆事件 {pendingId.Entry}");
            __result = EnterPendingEvent(run, pendingId);
            return false;
        }

        return true;
    }

    private static async Task EnterPendingEvent(RunManager run, ModelId pendingId)
    {
        try
        {
            var eventModel = SaveUtil.EventOrDeprecated(pendingId);
            await run.EnterRoom(new EventRoom(eventModel));
        }
        catch (Exception ex)
        {
            Entry.Logger.Error($"[KeyOfManyDoors] 进入回忆事件失败: {ex}");
        }
    }
}
