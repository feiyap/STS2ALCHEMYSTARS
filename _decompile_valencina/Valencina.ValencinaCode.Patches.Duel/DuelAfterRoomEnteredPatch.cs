using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Encounters;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Patches.Duel;

[HarmonyPatch(typeof(Hook), "AfterRoomEntered")]
internal static class DuelAfterRoomEnteredPatch
{
	private static void Postfix(AbstractRoom room, ref Task __result)
	{
		__result = ApplyDuelRulesAfterRoomEntered(__result, room);
	}

	private static async Task ApplyDuelRulesAfterRoomEntered(Task original, AbstractRoom room)
	{
		await original;
		CombatRoom combatRoom = (CombatRoom)(object)((room is CombatRoom) ? room : null);
		if (combatRoom == null || !(combatRoom.Encounter is DuelEncounter))
		{
			return;
		}
		ICombatState combatState = (ICombatState)(object)combatRoom.CombatState;
		IReadOnlyList<Player> players = combatState.Players;
		if (players.Count == 0)
		{
			return;
		}
		Player participant = ((IEnumerable<Player>)players).FirstOrDefault((Func<Player, bool>)((Player player) => player.Creature.IsAlive)) ?? players[0];
		foreach (Player item in players)
		{
			Creature creature = item.Creature;
			DuelHpMemory.SaveIfMissing(combatRoom, item, creature.CurrentHp);
			if (item == participant)
			{
				creature.SetCurrentHpInternal((decimal)creature.MaxHp);
				await CompatPowerCmd.Apply<DuelParticipantPower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), creature, 1m, creature, (CardModel?)null, silent: false);
			}
			else
			{
				await CompatPowerCmd.Apply<DuelSpectatorPower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), creature, 1m, creature, (CardModel?)null, silent: false);
			}
		}
		foreach (Creature item2 in combatState.Enemies.ToList())
		{
			if (item2.IsAlive)
			{
				await CompatPowerCmd.Apply<DuelEnemyPower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), item2, 1m, item2, (CardModel?)null, silent: false);
			}
		}
	}
}
