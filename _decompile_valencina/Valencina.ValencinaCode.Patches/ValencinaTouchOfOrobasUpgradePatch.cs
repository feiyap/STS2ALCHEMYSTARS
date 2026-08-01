using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using Valencina.ValencinaCode.Precognition;
using Valencina.ValencinaCode.Relics;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(TouchOfOrobas), "GetUpgradedStarterRelic")]
internal static class ValencinaTouchOfOrobasUpgradePatch
{
	private static void Postfix(RelicModel starterRelic, ref RelicModel __result)
	{
		if (!(starterRelic is ImperfectForesightEye))
		{
			return;
		}
		if (!((AbstractModel)starterRelic).IsMutable)
		{
			__result = (RelicModel)(object)ModelDb.Relic<CompleteForesightEye>();
			return;
		}
		CompleteForesightEye completeForesightEye = (CompleteForesightEye)(object)((RelicModel)ModelDb.Relic<CompleteForesightEye>()).ToMutable();
		if (starterRelic is IValencinaCounterLevelSource valencinaCounterLevelSource)
		{
			completeForesightEye.CounterLevel = valencinaCounterLevelSource.CounterLevel;
		}
		__result = (RelicModel)(object)completeForesightEye;
	}
}
