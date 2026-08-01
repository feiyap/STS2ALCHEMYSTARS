using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using Valencina.ValencinaCode.Relics;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(TouchOfOrobas), "AfterObtained")]
internal static class ValencinaTouchOfOrobasAfterObtainedPatch
{
	private static bool Prefix(TouchOfOrobas __instance, ref Task __result)
	{
		if (!OrobasValencinaPatch.IsValencina(((RelicModel)__instance).Owner))
		{
			return true;
		}
		__result = AfterObtainedForValencina(__instance);
		return false;
	}

	private static async Task AfterObtainedForValencina(TouchOfOrobas relic)
	{
		ImperfectForesightEye relic2 = ((RelicModel)relic).Owner.GetRelic<ImperfectForesightEye>();
		if (relic2 != null)
		{
			int counterLevel = relic2.CounterLevel;
			CompleteForesightEye completeForesightEye = (CompleteForesightEye)(object)((RelicModel)ModelDb.Relic<CompleteForesightEye>()).ToMutable();
			completeForesightEye.CounterLevel = counterLevel;
			OrobasValencinaPatch.MarkRelicDiscovered(((RelicModel)relic).Owner, (RelicModel)(object)completeForesightEye);
			await RelicCmd.Replace((RelicModel)(object)relic2, (RelicModel)(object)completeForesightEye);
		}
	}
}
