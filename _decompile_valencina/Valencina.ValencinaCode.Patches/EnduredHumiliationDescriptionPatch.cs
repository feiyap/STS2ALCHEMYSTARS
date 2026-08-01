using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Cards;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
public static class EnduredHumiliationDescriptionPatch
{
	private static MethodBase? TargetMethod()
	{
		Type type = AccessTools.Inner(typeof(CardModel), "DescriptionPreviewType");
		if (type == null)
		{
			return null;
		}
		return AccessTools.Method(typeof(CardModel), "GetDescriptionForPile", new Type[3]
		{
			typeof(PileType),
			type,
			typeof(Creature)
		}, (Type[])null);
	}

	private static void Postfix(CardModel __instance, ref string __result)
	{
		if (__instance is EnduredHumiliation && !string.IsNullOrEmpty(__result))
		{
			int num = ReadCurrentFloorNumber() / 15;
			if (num > 0 && !__result.Contains("当前额外段数", StringComparison.Ordinal) && !__result.Contains("Current extra hits", StringComparison.OrdinalIgnoreCase))
			{
				bool flag = __result.IndexOfAny(new char[5] { '造', '成', '伤', '害', '层' }) >= 0;
				__result += (flag ? $"\n（当前额外段数：{num}）" : $"\n(Current extra hits: {num})");
			}
		}
	}

	private static int ReadCurrentFloorNumber()
	{
		try
		{
			Type type = typeof(CardModel).Assembly.GetType("MegaCrit.Sts2.Core.Runs.RunManager");
			if (type == null)
			{
				return 0;
			}
			object obj = null;
			string[] array = new string[6] { "Instance", "instance", "Singleton", "singleton", "Main", "main" };
			foreach (string name in array)
			{
				obj = type.GetProperty(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null) ?? obj;
				if (obj != null)
				{
					break;
				}
				obj = type.GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null) ?? obj;
				if (obj != null)
				{
					break;
				}
			}
			if (obj != null)
			{
				array = new string[4] { "FloorNum", "floorNum", "CurrentFloor", "currentFloor" };
				foreach (string name2 in array)
				{
					if (type.GetProperty(name2, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(obj) is IConvertible convertible)
					{
						return Math.Max(0, convertible.ToInt32(null));
					}
					if (type.GetField(name2, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(obj) is IConvertible convertible2)
					{
						return Math.Max(0, convertible2.ToInt32(null));
					}
				}
			}
			array = new string[4] { "FloorNum", "floorNum", "CurrentFloor", "currentFloor" };
			foreach (string name3 in array)
			{
				if (type.GetProperty(name3, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null) is IConvertible convertible3)
				{
					return Math.Max(0, convertible3.ToInt32(null));
				}
				if (type.GetField(name3, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null) is IConvertible convertible4)
				{
					return Math.Max(0, convertible4.ToInt32(null));
				}
			}
		}
		catch
		{
		}
		return 0;
	}
}
