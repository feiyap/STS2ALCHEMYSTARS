using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Audio;
using Valencina.ValencinaCode.Monsters;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
internal static class ValencinaBossMusicCombatSetupPatch
{
	private static MethodBase? TargetMethod()
	{
		return AccessTools.Method(typeof(CombatManager), "SetUpCombat", new Type[1] { typeof(CombatState) }, (Type[])null);
	}

	private static void Postfix(CombatState state)
	{
		ValencinaMusicManager.StartBossMusicIfNeeded(state);
		RestorePlayersForKaiserCombat(state);
		CleanupStaleKaiserPhaseChoiceLocks(state);
	}

	private static void RestorePlayersForKaiserCombat(CombatState state)
	{
		if (state.Enemies.Any((Creature enemy) => ((enemy != null) ? enemy.Monster : null) is UngezieferKaiser))
		{
			TaskHelper.RunSafely(RestorePlayersForKaiserCombatAsync(state));
		}
	}

	private static async Task RestorePlayersForKaiserCombatAsync(CombatState state)
	{
		foreach (Player player in state.Players)
		{
			if (((player != null) ? player.Creature : null) != null && player.Creature.MaxHp > 0)
			{
				await CreatureCmd.SetCurrentHp(player.Creature, (decimal)player.Creature.MaxHp);
			}
		}
		MainFile.Logger.Info("[UngezieferKaiser] Restored all players to full HP at combat start.", 1);
	}

	private static void CleanupStaleKaiserPhaseChoiceLocks(CombatState state)
	{
		if (!UngezieferKaiser.HasActivePhaseTransitionChoice((ICombatState?)(object)state))
		{
			TaskHelper.RunSafely(CleanupStaleKaiserPhaseChoiceLocksAsync(state));
		}
	}

	private static async Task CleanupStaleKaiserPhaseChoiceLocksAsync(CombatState state)
	{
		foreach (Creature item in state.Players.Select((Player player) => player.Creature))
		{
			KaiserPhaseChoiceInputLockPower power = item.GetPower<KaiserPhaseChoiceInputLockPower>();
			if (power != null)
			{
				await PowerCmd.Remove((PowerModel)(object)power);
			}
		}
	}
}
