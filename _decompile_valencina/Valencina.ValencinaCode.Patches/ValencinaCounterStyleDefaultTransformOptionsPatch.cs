using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Cards;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(CardFactory), "GetDefaultTransformationOptions")]
internal static class ValencinaCounterStyleDefaultTransformOptionsPatch
{
	private static void Postfix(ref IEnumerable<CardModel> __result)
	{
		__result = __result.Where((CardModel card) => !(card is ICounterStyleCard));
	}
}
