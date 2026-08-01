using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Valencina.ValencinaCode.Systems;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NCombatRoom), "AddCreature")]
public static class ValencinaAmmoAddCreaturePatch
{
	public static void Postfix(NCombatRoom __instance, Creature __0)
	{
		if (ValencinaAmmoCombatStartPatch.ShouldUseAmmoSystemForRoom(__instance, "add-creature"))
		{
			if (!ValencinaAmmoCombatStartPatch.WasInitializedAsActiveCombat(__instance))
			{
				ValencinaProbeLog.Info("ammo-add-creature-lazy-init", "Starting lazy ammo init for active room that was not ready during NCombatRoom._Ready. " + ValencinaAmmoCombatStartPatch.DescribeRoom(__instance) + " creature=" + (((__0 != null) ? __0.Name : null) ?? "null"), 20);
				ValencinaAmmoCombatStartPatch.TryStartInitialization(__instance, "add-creature-lazy");
			}
			else
			{
				ValencinaAmmoCombatStartPatch.RunLoggedAsync(RegisterAfterInitializationAsync(__instance, __0), "ammo creature registration");
			}
		}
	}

	private static async Task RegisterAfterInitializationAsync(NCombatRoom combatRoom, Creature creature)
	{
		await ValencinaAmmoCombatStartPatch.GetInitializationTask(combatRoom);
		if (ValencinaAmmoCombatStartPatch.ShouldUseAmmoSystemForRoom(combatRoom, "add-creature-after-init"))
		{
			await AmmoSystem.TryRegisterCombatCreatureAsync(creature);
		}
	}
}
