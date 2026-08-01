using System.Linq;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Cards;

namespace Valencina.ValencinaCode.Events;

internal static class VagrantEventState
{
	private sealed class ForceState
	{
		internal int Depth;
	}

	private static readonly ConditionalWeakTable<IRunState, ForceState> ForcedRuns = new ConditionalWeakTable<IRunState, ForceState>();

	internal static bool IsForcedAllowed(IRunState runState)
	{
		if (ForcedRuns.TryGetValue(runState, out ForceState value))
		{
			return value.Depth > 0;
		}
		return false;
	}

	internal static bool CheckAllowedForced(IRunState runState, EventModel eventModel)
	{
		ForceState orCreateValue = ForcedRuns.GetOrCreateValue(runState);
		orCreateValue.Depth++;
		try
		{
			return eventModel.IsAllowed(runState);
		}
		finally
		{
			orCreateValue.Depth--;
		}
	}

	internal static bool WasOldVagrantEventVisited(IRunState runState)
	{
		RunState val = (RunState)(object)((runState is RunState) ? runState : null);
		if (val != null)
		{
			return val.VisitedEventIds.Any((ModelId id) => id.Entry == "VAGRANT" || id.Entry == "VALENCINA-VAGRANT" || id.Entry == "VALENCINASTS2-VAGRANT" || id.Entry == "VALENCINA_VAGRANT" || id.Entry == "VALENCINASTS2_VAGRANT");
		}
		return false;
	}

	internal static bool AnyPlayerAlreadyHasVagrantReward(IRunState runState)
	{
		return ((IPlayerCollection)runState).Players.Any((Player player) => player.Deck.Cards.Any((CardModel card) => card is Vagrant || card is Lucio));
	}
}
