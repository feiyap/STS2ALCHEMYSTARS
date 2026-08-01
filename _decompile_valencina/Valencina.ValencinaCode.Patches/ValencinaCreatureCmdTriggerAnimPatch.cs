using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using Valencina.ValencinaCode.Character;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(CreatureCmd), "TriggerAnim")]
internal static class ValencinaCreatureCmdTriggerAnimPatch
{
	private static bool Prefix(Creature creature, string triggerName, float waitTime, ref Task __result)
	{
		Player player = creature.Player;
		if (!(((player != null) ? player.Character : null) is Valencina.ValencinaCode.Character.Valencina))
		{
			return true;
		}
		if (!string.Equals(triggerName, "Attack", StringComparison.Ordinal))
		{
			return true;
		}
		if (ValencinaAnimation.AreCustomVisualsSuppressedForTeardown())
		{
			ValencinaAnimation.ClearAttackCommandAnimationState(creature);
			return true;
		}
		__result = ValencinaAnimation.PlayAttackFromCommand(creature, waitTime);
		return false;
	}
}
