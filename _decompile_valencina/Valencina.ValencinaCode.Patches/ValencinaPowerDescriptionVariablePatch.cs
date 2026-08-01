using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(PowerModel), "AddDumbVariablesToDescription")]
internal static class ValencinaPowerDescriptionVariablePatch
{
	private static void Postfix(PowerModel __instance, LocString description)
	{
		if (__instance is IAddDumbVariablesToPowerDescription addDumbVariablesToPowerDescription)
		{
			addDumbVariablesToPowerDescription.AddDumbVariablesToPowerDescription(description);
		}
	}
}
