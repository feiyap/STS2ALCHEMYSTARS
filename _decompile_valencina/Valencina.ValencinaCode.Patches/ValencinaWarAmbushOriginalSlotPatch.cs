using System.Collections.Generic;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using Valencina.ValencinaCode.Encounters;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(EncounterModel), "CreateScene")]
internal static class ValencinaWarAmbushOriginalSlotPatch
{
	private static void Postfix(EncounterModel __instance, ref Control __result)
	{
		if (!(__instance is WarAmbushEncounter) || __result == null)
		{
			return;
		}
		IReadOnlyList<(MonsterModel, string)> monstersWithSlots = __instance.MonstersWithSlots;
		if (monstersWithSlots.All<(MonsterModel, string)>(((MonsterModel Monster, string Slot) entry) => entry.Monster is Wriggler))
		{
			SetMarker(__result, "wriggler1", 1115f, 685f);
			SetMarker(__result, "wriggler2", 1336f, 706f);
			SetMarker(__result, "wriggler3", 1577f, 714f);
			SetMarker(__result, "wriggler4", 1802f, 690f);
			return;
		}
		if (monstersWithSlots.Any<(MonsterModel, string)>(((MonsterModel Monster, string Slot) entry) => entry.Monster is BowlbugRock))
		{
			if (monstersWithSlots.Count == 2)
			{
				SetMarker(__result, "odd", 1225f, 686f);
				SetMarker(__result, "even", 1540f, 713f);
			}
			else
			{
				SetMarker(__result, "first", 1099f, 673f);
				SetMarker(__result, "middle", 1368f, 712f);
				SetMarker(__result, "last", 1666f, 675f);
			}
			return;
		}
		if (monstersWithSlots.All<(MonsterModel, string)>(((MonsterModel Monster, string Slot) entry) => entry.Monster is Exoskeleton))
		{
			if (monstersWithSlots.Count == 3)
			{
				SetMarker(__result, "first", 1165f, 691f);
				SetMarker(__result, "second", 1401f, 756f);
				SetMarker(__result, "third", 1643f, 712f);
			}
			else
			{
				SetMarker(__result, "first", 1105f, 705f);
				SetMarker(__result, "second", 1312f, 737f);
				SetMarker(__result, "third", 1519f, 724f);
				SetMarker(__result, "fourth", 1716f, 699f);
			}
			return;
		}
		if (monstersWithSlots.All<(MonsterModel, string)>(((MonsterModel Monster, string Slot) entry) => entry.Monster is Myte))
		{
			SetMarker(__result, "first", 1245f, 711f);
			SetMarker(__result, "second", 1736f, 737f);
			return;
		}
		bool flag = monstersWithSlots.Count == 1;
		if (flag)
		{
			MonsterModel item = monstersWithSlots[0].Item1;
			bool flag2 = ((item is ShrinkerBeetle || item is FuzzyWurmCrawler) ? true : false);
			flag = flag2;
		}
		if (flag)
		{
			SetMarker(__result, "single", 1440f, 740f);
		}
	}

	private static void SetMarker(Control root, string name, float x, float y)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		((Node2D)((Node)root).GetNode<Marker2D>(NodePath.op_Implicit(name))).Position = new Vector2(x, y);
	}
}
