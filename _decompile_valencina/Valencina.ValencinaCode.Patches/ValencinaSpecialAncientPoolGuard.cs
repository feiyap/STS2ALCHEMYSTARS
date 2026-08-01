using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Events;
using Valencina.ValencinaCode.Relics.Rien;

namespace Valencina.ValencinaCode.Patches;

internal static class ValencinaSpecialAncientPoolGuard
{
	private static readonly FieldInfo? RoomsField = AccessTools.Field(typeof(ActModel), "_rooms");

	internal static IEnumerable<AncientEventModel> FilterSharedAncientPool(IEnumerable<AncientEventModel> ancients)
	{
		return ancients.Where((AncientEventModel ancient) => !IsFollowUpAncient(ancient) && !IsAct4OnlyAncient(ancient));
	}

	internal static bool RepairGeneratedAncient(IRunState runState, ActModel act, Rng rng, bool log)
	{
		AncientEventModel ancient;
		try
		{
			ancient = act.Ancient;
		}
		catch (InvalidOperationException)
		{
			return false;
		}
		if (!IsFollowUpAncient(ancient))
		{
			return false;
		}
		int num = -1;
		for (int i = 0; i < runState.Acts.Count; i++)
		{
			if (runState.Acts[i] == act)
			{
				num = i;
				break;
			}
		}
		List<AncientEventModel> list = (from candidate in act.GetUnlockedAncients(runState.UnlockState)
			where !IsFollowUpAncient(candidate)
			where ((EventModel)candidate).IsAllowed(runState)
			select candidate).DistinctBy((AncientEventModel candidate) => ((AbstractModel)candidate).Id).ToList();
		if (list.Count == 0 && num > 0)
		{
			list = (from candidate in runState.UnlockState.SharedAncients
				where !IsFollowUpAncient(candidate)
				where ((EventModel)candidate).IsAllowed(runState)
				select candidate).DistinctBy((AncientEventModel candidate) => ((AbstractModel)candidate).Id).ToList();
		}
		if (list.Count == 0)
		{
			MainFile.Logger.Warn($"[RienSecondAncient] Could not replace leaked follow-up Ancient '{((AbstractModel)ancient).Id}' in act '{((AbstractModel)act).Id}': no valid replacement was available.", 1);
			return false;
		}
		object? obj = RoomsField?.GetValue(act);
		RoomSet val = (RoomSet)((obj is RoomSet) ? obj : null);
		if (val == null)
		{
			MainFile.Logger.Warn("[RienSecondAncient] Could not replace leaked follow-up Ancient: ActModel._rooms was not found.", 1);
			return false;
		}
		AncientEventModel val2 = (val.Ancient = rng.NextItem<AncientEventModel>((IEnumerable<AncientEventModel>)list));
		if (log)
		{
			MainFile.Logger.Info($"[RienSecondAncient] Replaced leaked follow-up Ancient '{((AbstractModel)ancient).Id}' in act '{((AbstractModel)act).Id}' with '{((AbstractModel)val2).Id}'.", 1);
		}
		return true;
	}

	internal static bool RepairCurrentRunAncients(bool log)
	{
		try
		{
			if (RunManager.Instance == null || !UngezieferKaiserFinalBossController.TryGetRunState(RunManager.Instance, out IRunState runState))
			{
				return false;
			}
			bool flag = false;
			foreach (ActModel act in runState.Acts)
			{
				flag |= RepairGeneratedAncient(runState, act, runState.Rng.UpFront, log);
			}
			return flag;
		}
		catch (Exception value)
		{
			MainFile.Logger.Warn($"[RienSecondAncient] Failed to repair current run Ancients: {value}", 1);
			return false;
		}
	}

	internal static bool IsFollowUpAncient(AncientEventModel ancient)
	{
		if ((ancient is ThumbAdvisor || ancient is LimbusCompanyHeadquarters || ancient is Rien) ? true : false)
		{
			return true;
		}
		string entry = ((AbstractModel)ancient).Id.Entry;
		if (!entry.Contains("THUMB_ADVISOR", StringComparison.OrdinalIgnoreCase) && !entry.Contains("LIMBUS_COMPANY_HEADQUARTERS", StringComparison.OrdinalIgnoreCase) && !entry.Equals("RIEN", StringComparison.OrdinalIgnoreCase) && !entry.EndsWith("-RIEN", StringComparison.OrdinalIgnoreCase))
		{
			return entry.EndsWith("_RIEN", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static bool IsAct4OnlyAncient(AncientEventModel ancient)
	{
		if (ancient is Stars)
		{
			return true;
		}
		string entry = ((AbstractModel)ancient).Id.Entry;
		if (!entry.Equals("STARS", StringComparison.OrdinalIgnoreCase) && !entry.EndsWith("-STARS", StringComparison.OrdinalIgnoreCase))
		{
			return entry.EndsWith("_STARS", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	internal static bool IsKaiserSummonRelicType(Type type)
	{
		if (!(type == typeof(Maggot)) && !(type == typeof(Moth)) && !(type == typeof(Fly)) && !type.Name.Equals("Maggot", StringComparison.Ordinal) && !type.Name.Equals("Moth", StringComparison.Ordinal))
		{
			return type.Name.Equals("Fly", StringComparison.Ordinal);
		}
		return true;
	}
}
