using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Scaffolding.Content;
using Valencina.ValencinaCode.Acts;
using Valencina.ValencinaCode.Character;
using Valencina.ValencinaCode.Encounters;
using Valencina.ValencinaCode.Relics.Rien;
using Valencina.ValencinaCode.Settings;

namespace Valencina.ValencinaCode.Patches;

internal static class UngezieferKaiserFinalBossController
{
	private static readonly PropertyInfo? RunStateActsProperty = AccessTools.Property(typeof(RunState), "Acts");

	private static readonly PropertyInfo? RunManagerStateProperty = AccessTools.Property(typeof(RunManager), "State");

	internal static bool EnsureAct4Slot(IRunState runState, bool log)
	{
		if (!ShouldUseAct4Route(runState, out string details))
		{
			RemoveAct4SlotIfUnavailable(runState, log);
			if (log)
			{
				MainFile.Logger.Info("[UngezieferKaiser] Act 4 insertion skipped. " + details, 1);
			}
			return false;
		}
		ActModel val = ((IEnumerable<ActModel>)runState.Acts).FirstOrDefault((Func<ActModel, bool>)IsValencinaAct4);
		if (val != null)
		{
			EnsureAct4RoomsGenerated(runState, val, log);
			NormalizeAct4Rooms(val);
			return true;
		}
		RunState val2 = (RunState)(object)((runState is RunState) ? runState : null);
		if (val2 == null || RunStateActsProperty == null)
		{
			if (log)
			{
				MainFile.Logger.Warn("[UngezieferKaiser] Could not append Act 4: mutable RunState.Acts property was unavailable.", 1);
			}
			return false;
		}
		List<ActModel> list = runState.Acts.ToList();
		ActModel val3 = ((ActModel)ModelDb.Act<ValencinaAct4>()).ToMutable();
		val3.GenerateRooms(runState.Rng.UpFront, runState.UnlockState, ((IPlayerCollection)runState).Players.Count > 1);
		NormalizeAct4Rooms(val3);
		list.Add(val3);
		RunStateActsProperty.SetValue(val2, list);
		if (log)
		{
			MainFile.Logger.Info("[UngezieferKaiser] Added Valencina Act 4 after Kaiser key set completion.", 1);
		}
		return true;
	}

	internal static bool RemoveAct4SlotIfUnavailable(IRunState runState, bool log)
	{
		if (ShouldUseAct4Route(runState, out string _))
		{
			return false;
		}
		int num = runState.Acts.ToList().FindIndex(IsValencinaAct4);
		if (num < 0)
		{
			return false;
		}
		if (runState.CurrentActIndex >= num)
		{
			if (log)
			{
				MainFile.Logger.Warn("[UngezieferKaiser] Act 4 is unavailable, but the current run is already at or past Valencina Act 4. Skipping unsafe act removal.", 1);
			}
			return false;
		}
		RunState val = (RunState)(object)((runState is RunState) ? runState : null);
		if (val == null || RunStateActsProperty == null)
		{
			if (log)
			{
				MainFile.Logger.Warn("[UngezieferKaiser] Could not remove unavailable Act 4: mutable RunState.Acts property was unavailable.", 1);
			}
			return false;
		}
		List<ActModel> list = runState.Acts.ToList();
		list.RemoveAt(num);
		RunStateActsProperty.SetValue(val, list);
		if (log)
		{
			MainFile.Logger.Info("[UngezieferKaiser] Removed Valencina Act 4 because the route is unavailable.", 1);
		}
		return true;
	}

	internal static void EnsureAct4RoomsGenerated(IRunState runState, ActModel act, bool log)
	{
		try
		{
			_ = act.BossEncounter;
			NormalizeAct4Rooms(act);
		}
		catch (InvalidOperationException ex) when (ex.Message.Contains("RoomSet.Boss", StringComparison.OrdinalIgnoreCase))
		{
			act.GenerateRooms(runState.Rng.UpFront, runState.UnlockState, ((IPlayerCollection)runState).Players.Count > 1);
			NormalizeAct4Rooms(act);
			if (log)
			{
				MainFile.Logger.Info("[UngezieferKaiser] Generated missing rooms for existing Valencina Act 4.", 1);
			}
		}
	}

	private static void NormalizeAct4Rooms(ActModel act)
	{
		if (act is ValencinaAct4 valencinaAct)
		{
			valencinaAct.NormalizeFixedRouteRooms();
		}
		else
		{
			EnsureAct4Boss(act);
		}
	}

	internal static bool RepairAccidentalKaiserBossSelections(IRunState runState, bool log)
	{
		bool flag = false;
		for (int i = 0; i < runState.Acts.Count; i++)
		{
			ActModel act = runState.Acts[i];
			flag |= RepairAccidentalKaiserBossSlot(runState, act, secondBoss: false, log);
			flag |= RepairAccidentalKaiserBossSlot(runState, act, secondBoss: true, log);
		}
		return flag;
	}

	private static bool RepairAccidentalKaiserBossSlot(IRunState runState, ActModel act, bool secondBoss, bool log)
	{
		EncounterModel val = (secondBoss ? act.SecondBossEncounter : act.BossEncounter);
		if (val == null || !IsKaiserEncounterId(val))
		{
			return false;
		}
		if (IsAllowedKaiserBossSlot(runState, act, secondBoss))
		{
			return false;
		}
		EncounterModel otherBoss = (secondBoss ? act.BossEncounter : act.SecondBossEncounter);
		EncounterModel[] array = (from candidate in act.AllBossEncounters
			where !IsKaiserEncounterId(candidate)
			where otherBoss == null || ((AbstractModel)candidate).Id != ((AbstractModel)otherBoss).Id
			where ModEncounterActValidityFilter.IsValidForAct(act, candidate)
			select candidate).ToArray();
		if (array.Length == 0)
		{
			MainFile.Logger.Warn($"[UngezieferKaiser] Could not replace accidental Kaiser {(secondBoss ? "second " : string.Empty)}boss in act '{((AbstractModel)act).Id}': no valid replacement boss was available.", 1);
			return false;
		}
		EncounterModel val2 = runState.Rng.UpFront.NextItem<EncounterModel>((IEnumerable<EncounterModel>)array);
		if (secondBoss)
		{
			act.SetSecondBossEncounter(val2);
		}
		else
		{
			act.SetBossEncounter(val2);
		}
		if (log)
		{
			MainFile.Logger.Info($"[UngezieferKaiser] Replaced accidental Kaiser {(secondBoss ? "second " : string.Empty)}boss in act '{((AbstractModel)act).Id}' with '{((AbstractModel)val2).Id}'.", 1);
		}
		return true;
	}

	private static void EnsureAct4Boss(ActModel act)
	{
		EncounterModel bossEncounter = (EncounterModel)(object)ModelDb.Encounter<UngezieferKaiserEncounter>();
		if (act.BossEncounter == null || !IsKaiserEncounterId(act.BossEncounter))
		{
			act.SetBossEncounter(bossEncounter);
		}
		if (act.SecondBossEncounter != null && IsKaiserEncounterId(act.SecondBossEncounter))
		{
			act.SetSecondBossEncounter((EncounterModel)null);
		}
	}

	internal static async Task TryApplyAndRegenerateCurrentMap(IRunState runState)
	{
		bool num = RemoveAct4SlotIfUnavailable(runState, log: true);
		bool flag = RepairAccidentalKaiserBossSelections(runState, log: true);
		bool flag2 = EnsureAct4Slot(runState, log: true);
		bool flag3 = runState.CurrentActIndex >= 0 && runState.CurrentActIndex < runState.Acts.Count && IsValencinaAct4(runState.Acts[runState.CurrentActIndex]);
		if ((num || flag || flag2) && flag3 && RunManager.Instance != null)
		{
			await RunManager.Instance.GenerateMap();
			MainFile.Logger.Info("[UngezieferKaiser] Regenerated current map after Kaiser route update.", 1);
		}
	}

	private static bool ShouldUseAct4Route(IRunState runState, out string details)
	{
		return HasRequiredKaiserKeys(runState, out details);
	}

	internal static bool HasRequiredKaiserKeys(IRunState runState, out string details)
	{
		if (!ValencinaModConfig.EnableKaiserContent)
		{
			details = "boss setting disabled";
			return false;
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		foreach (Player player in ((IPlayerCollection)runState).Players)
		{
			if (!(player.Character is Valencina.ValencinaCode.Character.Valencina) && !((AbstractModel)player.Character).Id.Entry.Contains("VALENCINA", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			flag = true;
			foreach (RelicModel relic in player.Relics)
			{
				if (relic is Maggot || RelicIdMatches(relic, "MAGGOT"))
				{
					flag2 = true;
				}
				if (relic is Moth || RelicIdMatches(relic, "MOTH"))
				{
					flag3 = true;
				}
				if (relic is Fly || RelicIdMatches(relic, "FLY"))
				{
					flag4 = true;
				}
			}
		}
		if (ValencinaModConfig.ForceUngezieferKaiserFinalBoss)
		{
			details = "forcedBySetting=true";
			return true;
		}
		details = $"hasValencina={flag}, maggot={flag2}, moth={flag3}, fly={flag4}";
		return flag && flag2 && flag3 && flag4;
	}

	private static bool RelicIdMatches(RelicModel relic, string expectedEntry)
	{
		string entry = ((AbstractModel)relic).Id.Entry;
		if (!entry.Equals(expectedEntry, StringComparison.OrdinalIgnoreCase) && !entry.Equals("VALENCINA-" + expectedEntry, StringComparison.OrdinalIgnoreCase) && !entry.Equals("VALENCINASTS2-" + expectedEntry, StringComparison.OrdinalIgnoreCase) && !entry.Equals("VALENCINA_" + expectedEntry, StringComparison.OrdinalIgnoreCase) && !entry.Equals("VALENCINASTS2_" + expectedEntry, StringComparison.OrdinalIgnoreCase) && !entry.EndsWith("-" + expectedEntry, StringComparison.OrdinalIgnoreCase))
		{
			return entry.EndsWith("_" + expectedEntry, StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	internal static bool IsKaiserEncounterId(EncounterModel encounter)
	{
		return ((AbstractModel)encounter).Id.Entry.Contains("UNGEZIEFER_KAISER", StringComparison.OrdinalIgnoreCase);
	}

	internal static bool IsValencinaAct4(ActModel act)
	{
		if (!(act is ValencinaAct4))
		{
			return ((AbstractModel)act).Id.Entry.Contains("VALENCINA_ACT4", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static bool IsAllowedKaiserBossSlot(IRunState runState, ActModel act, bool secondBoss)
	{
		if (!secondBoss)
		{
			return IsValencinaAct4(act);
		}
		return false;
	}

	internal static bool TryGetRunState(RunManager manager, [NotNullWhen(true)] out IRunState? runState)
	{
		runState = null;
		try
		{
			object? obj = RunManagerStateProperty?.GetValue(manager);
			runState = (IRunState?)((obj is IRunState) ? obj : null);
			return runState != null;
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[UngezieferKaiser] Failed to read RunManager.State by reflection: " + ex.Message, 1);
			return false;
		}
	}

	internal static async Task RepairCurrentRunAndRegenerateMapIfNeeded(string reason)
	{
		try
		{
			if (RunManager.Instance == null || !TryGetRunState(RunManager.Instance, out IRunState runState))
			{
				return;
			}
			ActModel val = ((runState.CurrentActIndex >= 0 && runState.CurrentActIndex < runState.Acts.Count) ? runState.Acts[runState.CurrentActIndex] : null);
			object obj;
			if (val == null)
			{
				obj = null;
			}
			else
			{
				EncounterModel bossEncounter = val.BossEncounter;
				obj = ((bossEncounter != null) ? ((AbstractModel)bossEncounter).Id.Entry : null);
			}
			string text = (string)obj;
			object obj2;
			if (val == null)
			{
				obj2 = null;
			}
			else
			{
				EncounterModel secondBossEncounter = val.SecondBossEncounter;
				obj2 = ((secondBossEncounter != null) ? ((AbstractModel)secondBossEncounter).Id.Entry : null);
			}
			string text2 = (string)obj2;
			bool num = RemoveAct4SlotIfUnavailable(runState, log: true);
			bool flag = RepairAccidentalKaiserBossSelections(runState, log: true);
			bool flag2 = EnsureAct4Slot(runState, log: true);
			if ((num || flag || flag2) && val != null)
			{
				EncounterModel bossEncounter2 = val.BossEncounter;
				int num2;
				if (!(text != ((bossEncounter2 != null) ? ((AbstractModel)bossEncounter2).Id.Entry : null)))
				{
					EncounterModel secondBossEncounter2 = val.SecondBossEncounter;
					num2 = ((text2 != ((secondBossEncounter2 != null) ? ((AbstractModel)secondBossEncounter2).Id.Entry : null)) ? 1 : 0);
				}
				else
				{
					num2 = 1;
				}
				if ((num2 != 0 || IsValencinaAct4(val)) && RunManager.Instance != null)
				{
					await RunManager.Instance.GenerateMap();
					MainFile.Logger.Info("[UngezieferKaiser] Regenerated current act map after Kaiser content repair (" + reason + ").", 1);
				}
			}
		}
		catch (Exception value)
		{
			MainFile.Logger.Warn($"[UngezieferKaiser] Failed to repair current run after Kaiser setting change: {value}", 1);
		}
	}
}
