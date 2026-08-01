using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Cards;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
public static class BoomerangShockwaveDescriptionPatch
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
		if (__instance is BoomerangShockwave && IsUpgraded(__instance) && !string.IsNullOrEmpty(__result) && !__result.Contains("抽牌堆顶", StringComparison.Ordinal) && !__result.Contains("抽牌堆顶部", StringComparison.Ordinal) && !__result.Contains("draw pile", StringComparison.OrdinalIgnoreCase))
		{
			bool flag = ContainsCjk(__result);
			__result += (flag ? "\n打出后，将本牌置于抽牌堆顶。" : "\nAfter use, place this card on top of your draw pile.");
		}
	}

	private static bool ContainsCjk(string text)
	{
		foreach (char c in text)
		{
			if (c >= '一' && c <= '鿿')
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsUpgraded(CardModel card)
	{
		string[] array = new string[4] { "IsUpgraded", "Upgraded", "isUpgraded", "upgraded" };
		foreach (string name in array)
		{
			PropertyInfo property = ((object)card).GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null)
			{
				try
				{
					if (property.GetValue(card) is bool result)
					{
						return result;
					}
				}
				catch
				{
				}
			}
			FieldInfo field = ((object)card).GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (!(field != null))
			{
				continue;
			}
			try
			{
				if (field.GetValue(card) is bool result2)
				{
					return result2;
				}
			}
			catch
			{
			}
		}
		return false;
	}
}
