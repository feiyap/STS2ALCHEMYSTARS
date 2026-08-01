using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Valencina.ValencinaCode.Character;
using Valencina.ValencinaCode.Systems;
using Valencina.ValencinaCode.UI;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NCombatRoom), "_Ready")]
public static class ValencinaAmmoCombatStartPatch
{
	private static readonly HashSet<NCombatRoom> InitializedActiveCombatRooms = new HashSet<NCombatRoom>();

	private static readonly Dictionary<NCombatRoom, Task> InitializationTasks = new Dictionary<NCombatRoom, Task>();

	public static void Postfix(NCombatRoom __instance)
	{
		TryStartInitialization(__instance, "ready");
	}

	internal static bool TryStartInitialization(NCombatRoom? combatRoom, string phase)
	{
		if (!ShouldUseAmmoSystemForRoom(combatRoom, phase) || combatRoom == null)
		{
			return false;
		}
		lock (InitializedActiveCombatRooms)
		{
			if (!InitializedActiveCombatRooms.Add(combatRoom))
			{
				ValencinaProbeLog.Warn("ammo-room-" + phase + "-duplicate", "Skipped duplicate ammo init for same combat room. " + DescribeRoom(combatRoom), 8);
				return true;
			}
		}
		Task value = RunLoggedAsync(InitializeAsync(combatRoom), "ammo combat initialization (" + phase + ")");
		lock (InitializedActiveCombatRooms)
		{
			InitializationTasks[combatRoom] = value;
		}
		return true;
	}

	private static async Task InitializeAsync(NCombatRoom combatRoom)
	{
		ValencinaProbeLog.Info("ammo-room-init-active", "Ammo init accepted for active combat room. " + DescribeRoom(combatRoom), 20);
		await AmmoSystem.EnterCombatAsync();
		AmmoUiSync.EnsureCombatUi((Node)(object)combatRoom);
		foreach (NCreature item in combatRoom.CreatureNodes.ToList())
		{
			await AmmoSystem.TryRegisterCombatCreatureAsync(item.Entity);
		}
		AmmoUiSync.RefreshAll(showFallbackLabel: false);
	}

	internal static bool WasInitializedAsActiveCombat(NCombatRoom? combatRoom)
	{
		if (combatRoom == null)
		{
			return false;
		}
		lock (InitializedActiveCombatRooms)
		{
			return InitializedActiveCombatRooms.Contains(combatRoom);
		}
	}

	internal static bool RemoveInitializedRoom(NCombatRoom? combatRoom)
	{
		if (combatRoom == null)
		{
			return false;
		}
		lock (InitializedActiveCombatRooms)
		{
			InitializationTasks.Remove(combatRoom);
			return InitializedActiveCombatRooms.Remove(combatRoom);
		}
	}

	internal static Task GetInitializationTask(NCombatRoom? combatRoom)
	{
		if (combatRoom == null)
		{
			return Task.CompletedTask;
		}
		lock (InitializedActiveCombatRooms)
		{
			Task value;
			return InitializationTasks.TryGetValue(combatRoom, out value) ? value : Task.CompletedTask;
		}
	}

	internal static bool ShouldUseAmmoSystemForRoom(NCombatRoom? combatRoom, string phase)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (combatRoom == null)
		{
			ValencinaProbeLog.Warn("ammo-room-" + phase + "-null", "Skipped ammo " + phase + ": NCombatRoom instance was null.", 8);
			return false;
		}
		if ((int)combatRoom.Mode != 0)
		{
			ValencinaProbeLog.Info($"ammo-room-{phase}-skip-{combatRoom.Mode}", "Skipped ammo " + phase + " for non-active combat room. " + DescribeRoom(combatRoom), 20);
			return false;
		}
		return true;
	}

	internal static string DescribeRoom(NCombatRoom? combatRoom)
	{
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		if (combatRoom == null)
		{
			return "room=null";
		}
		string value;
		try
		{
			value = string.Join(", ", combatRoom.CreatureNodes.Select(delegate(NCreature node)
			{
				Creature obj2 = ((node != null) ? node.Entity : null);
				string value5 = ((obj2 != null) ? obj2.Name : null) ?? "null";
				object obj3;
				if (obj2 == null)
				{
					obj3 = null;
				}
				else
				{
					Player player = obj2.Player;
					if (player == null)
					{
						obj3 = null;
					}
					else
					{
						CharacterModel character = player.Character;
						obj3 = ((character != null) ? ((AbstractModel)character).Id.Entry : null);
					}
				}
				if (obj3 == null)
				{
					obj3 = "non-player";
				}
				string value6 = (string)obj3;
				object obj4;
				if (obj2 == null)
				{
					obj4 = null;
				}
				else
				{
					Player player2 = obj2.Player;
					obj4 = ((player2 != null) ? player2.NetId.ToString() : null);
				}
				if (obj4 == null)
				{
					obj4 = "null";
				}
				string value7 = (string)obj4;
				object obj5;
				if (obj2 == null)
				{
					obj5 = null;
				}
				else
				{
					Player player3 = obj2.Player;
					obj5 = ((player3 != null) ? player3.Character : null);
				}
				bool value8 = obj5 is Valencina.ValencinaCode.Character.Valencina;
				return $"{value5}/{value6}/net={value7}/val={value8}";
			}));
		}
		catch (Exception ex)
		{
			value = $"<creature summary failed: {ex.GetType().Name}: {ex.Message}>";
		}
		bool value2;
		try
		{
			value2 = NCombatRoom.Instance == combatRoom;
		}
		catch
		{
			value2 = false;
		}
		string value3 = LocalContext.NetId?.ToString() ?? "null";
		bool value4 = IsEmbeddedEventCombatRoom(combatRoom);
		return $"mode={combatRoom.Mode}, currentInstance={value2}, localNetId={value3}, isEmbeddedEventCombat={value4}, creatures=[{value}]";
	}

	private static bool IsEmbeddedEventCombatRoom(NCombatRoom combatRoom)
	{
		try
		{
			NEventRoom instance = NEventRoom.Instance;
			return ((instance != null) ? instance.EmbeddedCombatRoom : null) == combatRoom;
		}
		catch
		{
			return false;
		}
	}

	internal static async Task RunLoggedAsync(Task task, string operation)
	{
		try
		{
			await task;
		}
		catch (Exception ex)
		{
			MainFile.Logger.Info($"[AmmoSystem] {operation} failed: {ex}", 1);
			ValencinaProbeLog.Warn("ammo-room-operation-exception", $"Ammo room operation failed. operation={operation}, exception={ex.GetType().Name}: {ex.Message}", 20);
		}
	}
}
