using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(CombatManager), "AfterCreatureAdded")]
internal static class ValencinaWarDefensePestPatch
{
	private const int DefensePestChancePercent = 10;

	private const int DefensePestStacks = 2;

	private static void Postfix(Creature creature, ref Task __result)
	{
		__result = ApplyAfterCreatureAdded(__result, creature);
	}

	private static async Task ApplyAfterCreatureAdded(Task original, Creature creature)
	{
		await original;
		ICombatState combatState = creature.CombatState;
		if (creature.IsEnemy && combatState != null && ValencinaWarDifficulty.IsActive(combatState.RunState) && creature.GetPower<DefensePestPower>() == null && combatState.RunState.Rng.Niche.NextInt(100) < 10)
		{
			await CompatPowerCmd.Apply<DefensePestPower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), creature, 2m, creature, (CardModel?)null, silent: false);
		}
	}
}
