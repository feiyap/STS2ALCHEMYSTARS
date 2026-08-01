using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(CombatRoom), "ToSerializable")]
internal static class ValencinaWarAmbushCombatRoomSerializePatch
{
	private static bool Prefix(CombatRoom __instance, ref SerializableRoom __result)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		if (!ValencinaWarAmbushEntryPatch.IsWarAmbushEncounter(__instance.Encounter))
		{
			return true;
		}
		SerializableRoom val = new SerializableRoom
		{
			RoomType = ((AbstractRoom)__instance).RoomType,
			EncounterId = ((AbstractModel)__instance.Encounter).Id,
			IsPreFinished = ((AbstractRoom)__instance).IsPreFinished,
			GoldProportion = __instance.GoldProportion,
			ParentEventId = __instance.ParentEventId,
			ShouldResumeParentEvent = __instance.ShouldResumeParentEventAfterCombat,
			EncounterState = __instance.Encounter.SaveCustomState()
		};
		foreach (var (val3, source) in __instance.ExtraRewards)
		{
			val.ExtraRewards[val3.NetId] = source.Select((Reward reward) => reward.ToSerializable()).ToList();
		}
		__result = val;
		return false;
	}
}
