using HarmonyLib;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(RewardsSet), "WithRewardsFromRoom")]
internal static class ValencinaWarAmbushRewardsPatch
{
	private static bool Prefix(RewardsSet __instance, AbstractRoom room, ref RewardsSet __result)
	{
		CombatRoom val = (CombatRoom)(object)((room is CombatRoom) ? room : null);
		if (val == null || !ValencinaWarAmbushEntryPatch.IsWarAmbushEncounter(val.Encounter))
		{
			return true;
		}
		__instance.EmptyForRoom(room);
		if (val.ExtraRewards.TryGetValue(__instance.Player, out var value))
		{
			__instance.Rewards.AddRange(value);
		}
		__result = __instance;
		return false;
	}
}
