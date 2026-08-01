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
	typeof(PileType),
	typeof(CardPilePosition),
	typeof(AbstractModel),
	typeof(bool)
})]
internal static class CounterStyleAddToDeckByPileTypePatch
{
	private static bool Prefix(CardModel card, PileType newPileType, ref Task<CardPileAddResult> __result)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if ((int)newPileType != 6 || !CounterStyleDeckReplacement.TryReplace(card, out var result))
		{
			return true;
		}
		__result = Task.FromResult<CardPileAddResult>(result);
		return false;
	}
}
