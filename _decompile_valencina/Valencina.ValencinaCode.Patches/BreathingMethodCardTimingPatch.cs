using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using Valencina.ValencinaCode.Cards;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(Hook), "AfterCardPlayed")]
internal static class BreathingMethodCardTimingPatch
{
	[HarmonyPostfix]
	private static void Postfix(ref Task __result, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		__result = ResolveAfterOriginalAsync(__result, choiceContext, cardPlay);
	}

	private static async Task ResolveAfterOriginalAsync(Task original, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await original;
		if (cardPlay.Card is ValencinaCard valencinaCard)
		{
			await valencinaCard.FlushPendingBreathingMethodGainAsync(choiceContext);
		}
	}
}
