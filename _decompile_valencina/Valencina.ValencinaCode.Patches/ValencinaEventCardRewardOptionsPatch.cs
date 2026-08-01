using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Cards;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(CardFactory), "CreateForReward", new Type[]
{
	typeof(Player),
	typeof(int),
	typeof(CardCreationOptions)
})]
internal static class ValencinaEventCardRewardOptionsPatch
{
	private static void Postfix(ref IEnumerable<CardCreationResult> __result)
	{
		__result = __result.Where(delegate(CardCreationResult result)
		{
			CardModel card = result.Card;
			return !(card is Lucio) && !(card is Vagrant);
		});
	}
}
