using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Valencina.ValencinaCode.Vfx;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NCreature), "OnPowerRemoved")]
internal static class ValencinaShinAuraPowerRemovedPatch
{
	private static void Postfix(NCreature __instance, PowerModel power)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (ShinAuraController.IsShinAuraPower(power))
		{
			Callable val = Callable.From((Action)delegate
			{
				ShinAuraController.Refresh(__instance);
			});
			((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
		}
	}
}
