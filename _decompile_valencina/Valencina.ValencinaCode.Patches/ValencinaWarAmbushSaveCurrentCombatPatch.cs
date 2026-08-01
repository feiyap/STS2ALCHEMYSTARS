using HarmonyLib;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(SaveManager), "SaveRun")]
internal static class ValencinaWarAmbushSaveCurrentCombatPatch
{
	private static void Prefix(ref AbstractRoom? preFinishedRoom)
	{
		if (preFinishedRoom == null)
		{
			RunState obj = RunManager.Instance.DebugOnlyGetState();
			AbstractRoom obj2 = ((obj != null) ? ((IRunState)obj).CurrentRoom : null);
			CombatRoom val = (CombatRoom)(object)((obj2 is CombatRoom) ? obj2 : null);
			if (val != null && ValencinaWarAmbushEntryPatch.IsWarAmbushEncounter(val.Encounter))
			{
				preFinishedRoom = (AbstractRoom?)(object)val;
			}
		}
	}
}
