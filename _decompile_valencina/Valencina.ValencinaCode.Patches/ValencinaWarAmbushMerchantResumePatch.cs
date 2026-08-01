using System;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(MerchantRoom), "Resume")]
internal static class ValencinaWarAmbushMerchantResumePatch
{
	private static bool Prefix(MerchantRoom __instance, AbstractRoom __0, IRunState? runState, ref Task __result)
	{
		CombatRoom val = (CombatRoom)(object)((__0 is CombatRoom) ? __0 : null);
		if (val == null || !ValencinaWarAmbushEntryPatch.IsWarAmbushEncounter(val.Encounter))
		{
			return true;
		}
		NRun instance = NRun.Instance;
		if (instance != null)
		{
			instance.SetCurrentRoom((Control)(object)NMerchantRoom.Create(__instance, ((runState != null) ? ((IPlayerCollection)runState).Players : null) ?? Array.Empty<Player>()));
		}
		__result = Task.CompletedTask;
		return false;
	}
}
