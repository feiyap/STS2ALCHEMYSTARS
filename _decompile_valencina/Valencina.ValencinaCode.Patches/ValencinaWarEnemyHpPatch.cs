using System;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Monsters;
using Valencina.ValencinaCode.Relics.Rien;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(CombatState), "CreateCreature")]
internal static class ValencinaWarEnemyHpPatch
{
	private static void Postfix(CombatState __instance, CombatSide side, ref Creature __result)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)side == 2 && ValencinaWarDifficulty.IsActive(__instance.RunState))
		{
			int num = __result.MaxHp;
			if (((IPlayerCollection)__instance.RunState).Players.Any((Player player) => player.GetRelic<Maggot>() != null))
			{
				num = (int)Math.Ceiling((decimal)num * 1.05m);
			}
			if (__instance.RunState.CurrentActIndex == 0 && IsActOneWarInsect(__result))
			{
				num = Math.Max(1, (int)Math.Floor((decimal)num * 0.5m));
			}
			bool flag = ValencinaWarAmbushEntryPatch.IsWarAmbushEncounter(__instance.Encounter);
			if (flag)
			{
				num = Math.Max(1, (int)Math.Floor((decimal)num * 0.75m));
			}
			if (__instance.RunState.CurrentActIndex == 2 && flag && IsVanillaWarInsect(__result))
			{
				num = Math.Max(1, (int)Math.Floor((decimal)num * 1.3m));
			}
			if (num != __result.MaxHp)
			{
				__result.SetMaxHpInternal((decimal)num);
				__result.SetCurrentHpInternal((decimal)num);
			}
		}
	}

	private static bool IsActOneWarInsect(Creature creature)
	{
		bool flag = IsVanillaWarInsect(creature);
		if (!flag)
		{
			MonsterModel monster = creature.Monster;
			bool flag2 = ((monster is GCompanySoldier || monster is GCompanyMinister) ? true : false);
			flag = flag2;
		}
		return flag;
	}

	private static bool IsVanillaWarInsect(Creature creature)
	{
		MonsterModel monster = creature.Monster;
		Wriggler val = (Wriggler)(object)((monster is Wriggler) ? monster : null);
		if (val != null)
		{
			if (!val.StartStunned)
			{
				goto IL_0063;
			}
		}
		else if (monster is BowlbugEgg || monster is BowlbugNectar || monster is BowlbugRock || monster is BowlbugSilk || monster is Exoskeleton || monster is Myte || monster is ThievingHopper || monster is ShrinkerBeetle || monster is FuzzyWurmCrawler)
		{
			goto IL_0063;
		}
		return false;
		IL_0063:
		return true;
	}
}
