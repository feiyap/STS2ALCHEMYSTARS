using HarmonyLib;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Systems.Duel;

namespace Valencina.ValencinaCode.Patches.Duel;

[HarmonyPatch(typeof(Hook), "ModifyGeneratedMap")]
internal static class DuelModifyGeneratedMapPatch
{
	private static void Postfix(IRunState runState, int actIndex, ref ActMap __result)
	{
		__result = DuelNodeSystem.ApplyDuelNodes(runState, __result, actIndex);
	}
}
