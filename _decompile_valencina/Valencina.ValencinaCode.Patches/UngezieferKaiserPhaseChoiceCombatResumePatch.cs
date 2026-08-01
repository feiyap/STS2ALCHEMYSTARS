using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Orbs;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Orbs;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Events;
using Valencina.ValencinaCode.Monsters;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(CombatRoom), "Resume")]
internal static class UngezieferKaiserPhaseChoiceCombatResumePatch
{
	private static ulong _fastRestoreVisualsUntilMsec;

	private static readonly FieldInfo? OrbManagerOrbsField = AccessTools.Field(typeof(NOrbManager), "_orbs");

	private static readonly FieldInfo? OrbManagerContainerField = AccessTools.Field(typeof(NOrbManager), "_orbContainer");

	private static readonly MethodInfo? OrbManagerTweenLayoutMethod = AccessTools.Method(typeof(NOrbManager), "TweenLayout", (Type[])null, (Type[])null);

	private static readonly MethodInfo? OrbManagerUpdateControllerNavigationMethod = AccessTools.Method(typeof(NOrbManager), "UpdateControllerNavigation", (Type[])null, (Type[])null);

	internal static bool IsRestoringPhaseCombatRoom { get; private set; }

	internal static bool ShouldFastRestoreVisuals
	{
		get
		{
			if (!IsRestoringPhaseCombatRoom)
			{
				return Time.GetTicksMsec() < _fastRestoreVisualsUntilMsec;
			}
			return true;
		}
	}

	private static void BeginFastVisualRestoreWindow()
	{
		_fastRestoreVisualsUntilMsec = Time.GetTicksMsec() + 1500;
	}

	private static bool Prefix(CombatRoom __instance, AbstractRoom __0, ref Task __result)
	{
		EventRoom val = (EventRoom)(object)((__0 is EventRoom) ? __0 : null);
		if (val == null || !(val.CanonicalEvent is CockroachEmperorPassiveDisableEvent))
		{
			return true;
		}
		if (((__instance != null) ? __instance.CombatState : null) == null)
		{
			return true;
		}
		NCombatRoom val2 = null;
		try
		{
			BeginFastVisualRestoreWindow();
			val2 = NCombatRoom.Create((ICombatRoomVisuals)(object)__instance, (CombatRoomMode)0);
			IsRestoringPhaseCombatRoom = true;
			NRun instance = NRun.Instance;
			if (instance != null)
			{
				instance.SetCurrentRoom((Control)(object)val2);
			}
		}
		catch (Exception value)
		{
			MainFile.Logger.Warn($"[UngezieferKaiser] SetCurrentRoom raised while restoring phase choice combat room; continuing restore. {value}", 1);
		}
		finally
		{
			IsRestoringPhaseCombatRoom = false;
		}
		try
		{
			if (((val2 != null) ? val2.Ui : null) != null)
			{
				val2.Ui.Activate(__instance.CombatState);
			}
		}
		catch (Exception value2)
		{
			MainFile.Logger.Warn($"[UngezieferKaiser] Failed to activate combat UI after phase choice. {value2}", 1);
		}
		try
		{
			if (val2 != null)
			{
				val2.SetUpBackground(__instance.CombatState.RunState);
			}
		}
		catch (Exception value3)
		{
			MainFile.Logger.Warn($"[UngezieferKaiser] Failed to restore combat background after phase choice. {value3}", 1);
		}
		try
		{
			ActiveScreenContext.Instance.Update();
		}
		catch (Exception value4)
		{
			MainFile.Logger.Warn($"[UngezieferKaiser] Failed to refresh active screen after phase choice. {value4}", 1);
		}
		RestoreCombatPlayState(__instance, val2);
		__result = Task.CompletedTask;
		return false;
	}

	private static void RestoreCombatPlayState(CombatRoom room, NCombatRoom? combatNode)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Invalid comparison between Unknown and I4
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Invalid comparison between Unknown and I4
		try
		{
			RunManager.Instance.ActionExecutor.Unpause();
			CombatManager.Instance.Unpause();
		}
		catch (Exception value)
		{
			MainFile.Logger.Warn($"[UngezieferKaiser] Failed to unpause combat after phase choice. {value}", 1);
		}
		try
		{
			if (CombatManager.Instance.IsInProgress && (int)room.CombatState.CurrentSide == 1 && !UngezieferKaiser.HasActivePhaseTransitionChoice((ICombatState?)(object)room.CombatState))
			{
				foreach (Player item in room.CombatState.Players.OrderBy(StablePlayerKey))
				{
					if (((item != null) ? item.PlayerCombatState : null) != null && !item.Creature.IsDead)
					{
						PlayerTurnPhase phase = item.PlayerCombatState.Phase;
						if ((int)phase <= 2)
						{
							item.PlayerCombatState.Phase = (PlayerTurnPhase)3;
						}
					}
				}
				RunManager.Instance.ActionQueueSynchronizer.SetCombatState((ActionSynchronizerCombatState)1);
			}
		}
		catch (Exception value2)
		{
			MainFile.Logger.Warn($"[UngezieferKaiser] Failed to restore combat play phase after phase choice. {value2}", 1);
		}
		try
		{
			if (((combatNode != null) ? combatNode.Ui : null) != null)
			{
				RebuildCurrentHand(combatNode.Ui.Hand, room);
				AccessTools.Method(typeof(NEndTurnButton), "OnTurnStarted", (Type[])null, (Type[])null)?.Invoke(combatNode.Ui.EndTurnButton, new object[1] { room.CombatState });
				AccessTools.Method(typeof(NPlayerHand), "AnimEnable", (Type[])null, (Type[])null)?.Invoke(combatNode.Ui.Hand, null);
				combatNode.EnableControllerNavigation();
				combatNode.Ui.Enable();
				RestoreOrbManagers(room, combatNode);
				RefreshMonsterIntents(room, combatNode);
				CleanupStalePhaseChoiceLocks(room);
				LogRestoredCombatState(room);
			}
		}
		catch (Exception value3)
		{
			MainFile.Logger.Warn($"[UngezieferKaiser] Failed to re-enable combat UI after phase choice. {value3}", 1);
		}
		try
		{
			ActiveScreenContext.Instance.Update();
		}
		catch (Exception value4)
		{
			MainFile.Logger.Warn($"[UngezieferKaiser] Failed to refresh active screen after re-enabling combat UI. {value4}", 1);
		}
	}

	private static void CleanupStalePhaseChoiceLocks(CombatRoom room)
	{
		if (!UngezieferKaiser.HasActivePhaseTransitionChoice((ICombatState?)(object)room.CombatState))
		{
			TaskHelper.RunSafely(CleanupStalePhaseChoiceLocksAsync(room));
		}
	}

	private static async Task CleanupStalePhaseChoiceLocksAsync(CombatRoom room)
	{
		foreach (Player item in room.CombatState.Players.OrderBy(StablePlayerKey))
		{
			object obj;
			if (item == null)
			{
				obj = null;
			}
			else
			{
				Creature creature = item.Creature;
				obj = ((creature != null) ? creature.GetPower<KaiserPhaseChoiceInputLockPower>() : null);
			}
			KaiserPhaseChoiceInputLockPower kaiserPhaseChoiceInputLockPower = (KaiserPhaseChoiceInputLockPower)obj;
			if (kaiserPhaseChoiceInputLockPower != null)
			{
				await PowerCmd.Remove((PowerModel)(object)kaiserPhaseChoiceInputLockPower);
			}
		}
	}

	private static string StablePlayerKey(Player? player)
	{
		object obj;
		if (player == null)
		{
			obj = null;
		}
		else
		{
			Creature creature = player.Creature;
			obj = ((creature == null) ? null : creature.CombatId?.ToString("D10"));
		}
		if (obj == null)
		{
			obj = ((player != null) ? player.NetId.ToString() : null) ?? string.Empty;
		}
		return (string)obj;
	}

	private static void RefreshMonsterIntents(CombatRoom room, NCombatRoom? combatNode)
	{
		if (combatNode == null)
		{
			return;
		}
		foreach (Creature item in room.CombatState.Creatures.Where((Creature creature) => creature != null && creature.IsAlive && creature.Monster != null))
		{
			NCreature creatureNode = combatNode.GetCreatureNode(item);
			if (creatureNode != null)
			{
				TaskHelper.RunSafely(creatureNode.RefreshIntents());
			}
		}
	}

	private static void LogRestoredCombatState(CombatRoom room)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		Player me = LocalContext.GetMe((ICombatState)(object)room.CombatState);
		int? obj;
		if (me == null)
		{
			obj = null;
		}
		else
		{
			PlayerCombatState playerCombatState = me.PlayerCombatState;
			obj = ((playerCombatState != null) ? new int?(playerCombatState.Hand.Cards.Count) : ((int?)null));
		}
		int value = obj ?? (-1);
		Logger logger = MainFile.Logger;
		DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(113, 5);
		defaultInterpolatedStringHandler.AppendLiteral("[UngezieferKaiser] Restored phase choice combat input. ");
		defaultInterpolatedStringHandler.AppendLiteral("side=");
		defaultInterpolatedStringHandler.AppendFormatted<CombatSide>(room.CombatState.CurrentSide);
		defaultInterpolatedStringHandler.AppendLiteral(", ");
		defaultInterpolatedStringHandler.AppendLiteral("playerPhase=");
		object obj2;
		if (me == null)
		{
			obj2 = null;
		}
		else
		{
			PlayerCombatState playerCombatState2 = me.PlayerCombatState;
			obj2 = ((playerCombatState2 != null) ? ((object)playerCombatState2.Phase/*cast due to .constrained prefix*/).ToString() : null);
		}
		if (obj2 == null)
		{
			obj2 = "<none>";
		}
		defaultInterpolatedStringHandler.AppendFormatted((string?)obj2);
		defaultInterpolatedStringHandler.AppendLiteral(", ");
		defaultInterpolatedStringHandler.AppendLiteral("syncState=");
		defaultInterpolatedStringHandler.AppendFormatted<ActionSynchronizerCombatState>(RunManager.Instance.ActionQueueSynchronizer.CombatState);
		defaultInterpolatedStringHandler.AppendLiteral(", ");
		defaultInterpolatedStringHandler.AppendLiteral("handCards=");
		defaultInterpolatedStringHandler.AppendFormatted(value);
		defaultInterpolatedStringHandler.AppendLiteral(", ");
		defaultInterpolatedStringHandler.AppendLiteral("currentRoom=");
		defaultInterpolatedStringHandler.AppendFormatted(((object)room.CombatState.RunState.CurrentRoom)?.GetType().Name ?? "<none>");
		defaultInterpolatedStringHandler.AppendLiteral(".");
		logger.Info(defaultInterpolatedStringHandler.ToStringAndClear(), 1);
	}

	private static void RestoreOrbManagers(CombatRoom room, NCombatRoom combatNode)
	{
		foreach (Player item in room.CombatState.Players.OrderBy(StablePlayerKey))
		{
			if (((item != null) ? item.Creature : null) == null || item.PlayerCombatState == null)
			{
				continue;
			}
			NCreature creatureNode = combatNode.GetCreatureNode(item.Creature);
			NOrbManager val = ((creatureNode != null) ? creatureNode.OrbManager : null);
			if (val != null)
			{
				try
				{
					RebuildOrbManager(item, val);
				}
				catch (Exception value)
				{
					MainFile.Logger.Warn($"[UngezieferKaiser] Failed to restore orb manager for player {item.NetId}. {value}", 1);
				}
			}
		}
	}

	private static void RebuildOrbManager(Player player, NOrbManager orbManager)
	{
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		if (player.PlayerCombatState == null || !(OrbManagerOrbsField?.GetValue(orbManager) is List<NOrb> list))
		{
			return;
		}
		object? obj = OrbManagerContainerField?.GetValue(orbManager);
		Control val = (Control)((obj is Control) ? obj : null);
		if (val == null)
		{
			return;
		}
		foreach (NOrb item in list.ToList())
		{
			GodotTreeExtensions.RemoveChildSafely((Node)(object)val, (Node)(object)item);
			GodotTreeExtensions.QueueFreeSafely((Node)(object)item);
		}
		list.Clear();
		OrbQueue orbQueue = player.PlayerCombatState.OrbQueue;
		int num = Math.Max(0, orbQueue.Capacity);
		for (int i = 0; i < num; i++)
		{
			OrbModel val2 = ((i < orbQueue.Orbs.Count) ? orbQueue.Orbs[i] : null);
			NOrb val3 = NOrb.Create(orbManager.IsLocal, val2);
			GodotTreeExtensions.AddChildSafely((Node)(object)val, (Node)(object)val3);
			list.Add(val3);
			((Control)val3).Position = Vector2.Zero;
		}
		OrbManagerTweenLayoutMethod?.Invoke(orbManager, null);
		OrbManagerUpdateControllerNavigationMethod?.Invoke(orbManager, null);
		orbManager.UpdateVisuals((OrbEvokeType)0);
	}

	private static void RebuildCurrentHand(NPlayerHand handNode, CombatRoom room)
	{
		Player me = LocalContext.GetMe((ICombatState)(object)room.CombatState);
		object obj;
		if (me == null)
		{
			obj = null;
		}
		else
		{
			PlayerCombatState playerCombatState = me.PlayerCombatState;
			obj = ((playerCombatState != null) ? playerCombatState.Hand : null);
		}
		CardPile val = (CardPile)obj;
		if (val == null)
		{
			return;
		}
		for (int i = 0; i < val.Cards.Count; i++)
		{
			if (handNode.GetCard(val.Cards[i]) == null)
			{
				NCard val2 = NCard.Create(val.Cards[i], (ModelVisibility)1);
				if (val2 != null)
				{
					handNode.Add(val2, i);
				}
			}
		}
	}
}
