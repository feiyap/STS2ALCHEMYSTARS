using System;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Monsters;
using Valencina.ValencinaCode.Settings;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(EncounterModel), "GenerateMonstersWithSlots")]
internal static class UngezieferKaiserEncounterReplacementPatch
{
	private static bool Prefix()
	{
		return true;
	}

	internal static bool ShouldReplace(EncounterModel encounter, IRunState runState)
	{
		return false;
	}

	internal static bool IsReplaceableFinalBossPoint(EncounterModel encounter, IRunState runState)
	{
		return false;
	}

	internal static bool IsCurrentMapPointTheLastBoss(IRunState runState)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (runState.Map == null)
		{
			return true;
		}
		MapCoord? currentMapCoord = runState.CurrentMapCoord;
		if (!currentMapCoord.HasValue)
		{
			return true;
		}
		MapCoord value;
		if (runState.Map.SecondBossMapPoint != null)
		{
			value = currentMapCoord.Value;
			return ((MapCoord)(ref value)).Equals(runState.Map.SecondBossMapPoint.coord);
		}
		value = currentMapCoord.Value;
		return ((MapCoord)(ref value)).Equals(runState.Map.BossMapPoint.coord);
	}

	internal static bool IsReplaced(EncounterModel encounter)
	{
		return false;
	}

	internal static bool HasKaiserMonster(EncounterModel encounter)
	{
		if (!encounter.HaveMonstersBeenGenerated)
		{
			return false;
		}
		try
		{
			return encounter.MonstersWithSlots.Any<(MonsterModel, string)>(((MonsterModel, string) pair) => pair.Item1 is UngezieferKaiser || ((AbstractModel)pair.Item1).Id.Entry.Contains("UNGEZIEFER_KAISER", StringComparison.OrdinalIgnoreCase) || ((object)pair.Item1).GetType().Name.Equals("UngezieferKaiser", StringComparison.Ordinal));
		}
		catch
		{
			return false;
		}
	}

	internal static bool ShouldUseKaiserAssets(EncounterModel encounter)
	{
		if (ValencinaModConfig.EnableKaiserContent)
		{
			if (!UngezieferKaiserFinalBossController.IsKaiserEncounterId(encounter))
			{
				return HasKaiserMonster(encounter);
			}
			return true;
		}
		return false;
	}
}
