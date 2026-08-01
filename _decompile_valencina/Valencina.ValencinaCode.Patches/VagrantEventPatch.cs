using System;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Character;
using Valencina.ValencinaCode.Events;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(Hook), "ModifyNextEvent")]
internal static class VagrantEventPatch
{
	private const float VagrantEventChance = 0.4f;

	private static void Postfix(IRunState runState, EventModel currentEvent, ref EventModel __result)
	{
		try
		{
			RunState val = (RunState)(object)((runState is RunState) ? runState : null);
			if (val != null && ((IPlayerCollection)runState).Players.Any((Player player) => player.Character is Valencina.ValencinaCode.Character.Valencina) && !IsValencinaAct4(runState) && !(((AbstractModel)__result).Id != ((AbstractModel)currentEvent).Id))
			{
				EventModel val2 = (EventModel)(object)ModelDb.Event<VagrantEvent>();
				if (!(((AbstractModel)currentEvent).Id == ((AbstractModel)val2).Id) && !val.VisitedEventIds.Contains(((AbstractModel)val2).Id) && !VagrantEventState.WasOldVagrantEventVisited(runState) && !VagrantEventState.AnyPlayerAlreadyHasVagrantReward(runState) && VagrantEventState.CheckAllowedForced(runState, val2) && ShouldReplaceWithVagrant(runState, val, currentEvent))
				{
					__result = val2;
					MainFile.Logger.Info($"[VagrantEvent] Replaced next event {((AbstractModel)currentEvent).Id.Entry} with {((AbstractModel)val2).Id.Entry}.", 1);
				}
			}
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[VagrantEvent] Failed to modify next event safely: " + ex.Message, 1);
		}
	}

	private static bool ShouldReplaceWithVagrant(IRunState runState, RunState concreteRunState, EventModel currentEvent)
	{
		return DeterministicRoll(runState, concreteRunState, currentEvent) < 40;
	}

	private static bool IsValencinaAct4(IRunState runState)
	{
		int currentActIndex = runState.CurrentActIndex;
		if (currentActIndex >= 0 && currentActIndex < runState.Acts.Count)
		{
			return ((AbstractModel)runState.Acts[currentActIndex]).Id.Entry.Contains("VALENCINA_ACT4", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	private static int DeterministicRoll(IRunState runState, RunState concreteRunState, EventModel currentEvent)
	{
		_003C_003Ey__InlineArray7<object> buffer = default(_003C_003Ey__InlineArray7<object>);
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray7<object>, object>(ref buffer, 0) = "ValencinaVagrantEvent";
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray7<object>, object>(ref buffer, 1) = runState.Rng.Seed;
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray7<object>, object>(ref buffer, 2) = concreteRunState.CurrentActIndex;
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray7<object>, object>(ref buffer, 3) = concreteRunState.TotalFloor;
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray7<object>, object>(ref buffer, 4) = concreteRunState.ActFloor;
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray7<object>, object>(ref buffer, 5) = concreteRunState.VisitedEventIds.Count;
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray7<object>, object>(ref buffer, 6) = ((AbstractModel)currentEvent).Id.Entry;
		return (int)(Math.Abs((long)StringHelper.GetDeterministicHashCode(string.Join("|", global::_003CPrivateImplementationDetails_003E.InlineArrayAsReadOnlySpan<_003C_003Ey__InlineArray7<object>, object>(in buffer, 7)))) % 100);
	}
}
