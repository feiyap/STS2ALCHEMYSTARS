using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using Valencina.ValencinaCode.Encounters;
using Valencina.ValencinaCode.Systems.Duel;

namespace Valencina.ValencinaCode.Patches.Duel;

[HarmonyPatch(typeof(RewardsSet), "WithRewardsFromRoom")]
internal static class DuelRewardsPatch
{
	private static bool Prefix(RewardsSet __instance, AbstractRoom room, ref RewardsSet __result)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		CombatRoom val = (CombatRoom)(object)((room is CombatRoom) ? room : null);
		if (val == null || !(val.Encounter is DuelEncounter))
		{
			return true;
		}
		__instance.EmptyForRoom(room);
		if (val.ExtraRewards.TryGetValue(__instance.Player, out var value))
		{
			__instance.Rewards.AddRange(value);
		}
		else
		{
			__instance.Rewards.Add((Reward)new GoldReward(75, __instance.Player, false));
			RelicModel val2 = DuelNodeSystem.CreateValencinaAncientReward(__instance.Player);
			__instance.Rewards.Add((Reward)((val2 != null) ? new RelicReward(val2, __instance.Player) : new RelicReward(ChooseUncommonOrRare(__instance.Player), __instance.Player)));
		}
		__result = __instance;
		return false;
	}

	private static RelicRarity ChooseUncommonOrRare(Player player)
	{
		if (player.PlayerRng.Rewards.NextFloat(1f) < 0.75f)
		{
			return (RelicRarity)3;
		}
		return (RelicRarity)4;
	}
}
