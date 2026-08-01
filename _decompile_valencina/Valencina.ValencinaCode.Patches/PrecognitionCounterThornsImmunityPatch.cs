using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Precognition;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(ThornsPower), "BeforeDamageReceived")]
internal static class PrecognitionCounterThornsImmunityPatch
{
	private static bool Prefix(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, ref Task __result)
	{
		if (cardSource is IPrecognitionVirtualCounterCard)
		{
			__result = Task.CompletedTask;
			return false;
		}
		return true;
	}
}
