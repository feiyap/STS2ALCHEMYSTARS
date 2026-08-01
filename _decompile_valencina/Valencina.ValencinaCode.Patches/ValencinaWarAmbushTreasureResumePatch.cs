using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(TreasureRoom), "Resume")]
internal static class ValencinaWarAmbushTreasureResumePatch
{
	private static bool Prefix(TreasureRoom __instance, AbstractRoom __0, IRunState? runState, ref Task __result)
	{
		CombatRoom val = (CombatRoom)(object)((__0 is CombatRoom) ? __0 : null);
		if (val == null || !ValencinaWarAmbushEntryPatch.IsWarAmbushEncounter(val.Encounter) || runState == null)
		{
			return true;
		}
		NRun instance = NRun.Instance;
		if (instance != null)
		{
			instance.SetCurrentRoom((Control)(object)NTreasureRoom.Create(__instance, runState));
		}
		__result = Task.CompletedTask;
		return false;
	}
}
