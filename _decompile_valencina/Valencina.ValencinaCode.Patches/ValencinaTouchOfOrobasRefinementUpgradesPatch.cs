using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
internal static class ValencinaTouchOfOrobasRefinementUpgradesPatch
{
	private static MethodBase? TargetMethod()
	{
		return AccessTools.PropertyGetter(typeof(TouchOfOrobas), "RefinementUpgrades") ?? AccessTools.Method(typeof(TouchOfOrobas), "get_RefinementUpgrades", (Type[])null, (Type[])null);
	}

	private static bool Prepare()
	{
		if (TargetMethod() != null)
		{
			return true;
		}
		MainFile.Logger.Warn("[OrobasValencinaPatch] TouchOfOrobas.RefinementUpgrades getter was not found; Complete Foresight Eye may be absent from upgrade previews.", 1);
		return false;
	}

	private static void Postfix(ref Dictionary<ModelId, RelicModel> __result)
	{
		OrobasValencinaPatch.AddForesightEyeRefinement(__result);
	}
}
