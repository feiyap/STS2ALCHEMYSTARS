using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;

namespace AlchemyStars.Patches.Enlightener;

/// <summary>
/// 记录本局是否已对某玩家触发过启迪者续页。
/// </summary>
internal static class EnlightenerFollowUpState
{
    private static readonly ConditionalWeakTable<IRunState, HashSet<ulong>> TriggeredPlayers = new();
    private static readonly object Gate = new();

    internal static bool TryMarkTriggered(Player? player)
    {
        if (player?.RunState == null)
            return false;

        lock (Gate)
        {
            return TriggeredPlayers.GetOrCreateValue(player.RunState).Add(player.NetId);
        }
    }
}
