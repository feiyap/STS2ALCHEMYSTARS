using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Cards;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
public static class PalermoExecutionDescriptionPatch
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

	private static void Prefix(CardModel __instance)
	{
		if (__instance is PalermoExecution palermoExecution)
		{
			palermoExecution.SyncCounterPreviewAmount();
		}
	}
}
