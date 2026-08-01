using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Valencina.ValencinaCode.Vfx;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NCreature), "OnPowerApplied")]
internal static class ValencinaShinAuraPowerAppliedPatch
{
	private static void Postfix(NCreature __instance, PowerModel power)
	{
		if (ShinAuraController.IsShinAuraPower(power))
		{
			ShinAuraController.Show(__instance);
		}
	}
}
