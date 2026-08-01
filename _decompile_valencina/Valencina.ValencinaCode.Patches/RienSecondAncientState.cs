using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Settings;

namespace Valencina.ValencinaCode.Patches;

internal static class RienSecondAncientState
{
	private static readonly ConditionalWeakTable<IRunState, HashSet<(ulong PlayerNetId, int ActIndex)>> TriggeredActs = new ConditionalWeakTable<IRunState, HashSet<(ulong, int)>>();

	private static readonly object Gate = new object();

	internal static bool TryMarkTriggered(Player? player)
	{
		if (!ValencinaModConfig.EnableRienFollowUpAncient)
		{
			return false;
		}
		if (((player != null) ? player.RunState : null) == null)
		{
			return false;
		}
		lock (Gate)
		{
			return TriggeredActs.GetOrCreateValue(player.RunState).Add((player.NetId, player.RunState.CurrentActIndex));
		}
	}
}
