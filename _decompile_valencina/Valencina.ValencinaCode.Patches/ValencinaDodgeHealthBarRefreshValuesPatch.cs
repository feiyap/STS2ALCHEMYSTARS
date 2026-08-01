using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NHealthBar), "RefreshValues")]
internal static class ValencinaDodgeHealthBarRefreshValuesPatch
{
	private static void Postfix(NHealthBar __instance)
	{
		DodgeHealthBarOverlay.Apply(__instance);
	}
}
