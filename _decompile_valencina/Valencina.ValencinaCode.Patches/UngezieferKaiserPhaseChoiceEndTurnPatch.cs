using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using Valencina.ValencinaCode.Monsters;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(PlayerCmd), "EndTurn")]
internal static class UngezieferKaiserPhaseChoiceEndTurnPatch
{
	private static bool Prefix(Player player)
	{
		if (((player != null) ? player.Creature : null) != null)
		{
			return !UngezieferKaiser.HasActivePhaseTransitionChoice(player.Creature.CombatState);
		}
		return true;
	}
}
