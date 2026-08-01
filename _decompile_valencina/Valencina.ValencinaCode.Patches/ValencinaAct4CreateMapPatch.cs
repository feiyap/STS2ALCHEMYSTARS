using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Acts;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(ActModel), "CreateMap")]
internal static class ValencinaAct4CreateMapPatch
{
	private static bool Prefix(ActModel __instance, RunState runState, ref ActMap __result)
	{
		if (!UngezieferKaiserFinalBossController.IsValencinaAct4(__instance))
		{
			return true;
		}
		UngezieferKaiserFinalBossController.EnsureAct4RoomsGenerated((IRunState)(object)runState, __instance, log: false);
		__result = (ActMap)(object)new ValencinaAct4Map();
		MainFile.Logger.Info("[UngezieferKaiser] Created Valencina Act 4 fixed map from ActModel.CreateMap.", 1);
		return false;
	}
}
