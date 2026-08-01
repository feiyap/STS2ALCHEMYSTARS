using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using Valencina.ValencinaCode.Relics;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(TouchOfOrobas), "SetupForPlayer")]
internal static class ValencinaTouchOfOrobasSetupPatch
{
	private static bool Prefix(TouchOfOrobas __instance, Player player, ref bool __result)
	{
		if (!OrobasValencinaPatch.IsValencina(player))
		{
			return true;
		}
		ImperfectForesightEye relic = player.GetRelic<ImperfectForesightEye>();
		if (relic == null)
		{
			__result = false;
			return false;
		}
		__instance.SetupForTests(((AbstractModel)relic).Id, ((AbstractModel)ModelDb.Relic<CompleteForesightEye>()).Id);
		__result = true;
		return false;
	}
}
