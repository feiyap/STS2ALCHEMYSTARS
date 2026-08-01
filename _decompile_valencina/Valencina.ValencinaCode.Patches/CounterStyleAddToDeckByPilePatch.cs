using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(CardPileCmd), "Add", new Type[]
{
	typeof(CardModel),
	typeof(CardPile),
	typeof(CardPilePosition),
	typeof(AbstractModel),
	typeof(bool)
})]
internal static class CounterStyleAddToDeckByPilePatch
{
	private static bool Prefix(CardModel card, CardPile newPile, ref Task<CardPileAddResult> __result)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if ((int)newPile.Type != 6 || !CounterStyleDeckReplacement.TryReplace(card, out var result))
		{
			return true;
		}
		__result = Task.FromResult<CardPileAddResult>(result);
		return false;
	}
}
