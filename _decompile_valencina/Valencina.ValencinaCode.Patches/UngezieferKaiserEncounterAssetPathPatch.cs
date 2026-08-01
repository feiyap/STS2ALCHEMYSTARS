using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Encounters;
using Valencina.ValencinaCode.Monsters;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(EncounterModel), "GetAssetPaths")]
internal static class UngezieferKaiserEncounterAssetPathPatch
{
	private static void Prefix(EncounterModel __instance, IRunState runState)
	{
		if (__instance.HaveMonstersBeenGenerated || !UngezieferKaiserEncounterReplacementPatch.ShouldReplace(__instance, runState))
		{
			return;
		}
		try
		{
			__instance.GenerateMonstersWithSlots(runState);
		}
		catch (InvalidOperationException ex) when (ex.Message.Contains("already been generated", StringComparison.OrdinalIgnoreCase))
		{
		}
	}

	private static void Postfix(EncounterModel __instance, ref IEnumerable<string> __result)
	{
		__result = __result.Select(UngezieferKaiserBackgroundRemapPatch.NormalizeSceneRemapPath);
		if (!UngezieferKaiserEncounterReplacementPatch.ShouldUseKaiserAssets(__instance))
		{
			if (IsAct4EliteEncounter(__instance))
			{
				__result = __result.Concat(Act4EliteAssets.AllAssetPaths).Select(UngezieferKaiserBackgroundRemapPatch.NormalizeSceneRemapPath).Distinct();
			}
		}
		else
		{
			__result = __result.Concat(UngezieferKaiserAssets.AllAssetPaths).Concat(IsAct4EliteEncounter(__instance) ? Act4EliteAssets.AllAssetPaths : Enumerable.Empty<string>()).Select(UngezieferKaiserBackgroundRemapPatch.NormalizeSceneRemapPath)
				.Distinct();
		}
	}

	private static bool IsAct4EliteEncounter(EncounterModel encounter)
	{
		ModelId id = ((AbstractModel)ModelDb.Encounter<ValencinaAct4EliteEncounter>()).Id;
		if (((AbstractModel)encounter).Id == id || encounter is ValencinaAct4EliteEncounter)
		{
			return true;
		}
		if (!encounter.HaveMonstersBeenGenerated)
		{
			return false;
		}
		try
		{
			return encounter.MonstersWithSlots.Any<(MonsterModel, string)>(delegate((MonsterModel, string) pair)
			{
				var (val, _) = pair;
				return ((val is Act4EliteRodya || val is Act4EliteHeathcliff || val is Act4EliteGregor) ? true : false) || ((AbstractModel)pair.Item1).Id.Entry.Contains("ACT4_ELITE_", StringComparison.OrdinalIgnoreCase);
			});
		}
		catch
		{
			return false;
		}
	}
}
