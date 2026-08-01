using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Flavor;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Character;
using Valencina.ValencinaCode.Settings;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
internal static class ValencinaMultiplayerPingSfxPatch
{
	private const ulong CooldownMs = 350uL;

	private static readonly FieldRef<FlavorSynchronizer, IPlayerCollection> PlayerCollectionRef = AccessTools.FieldRefAccess<FlavorSynchronizer, IPlayerCollection>("_playerCollection");

	private static readonly FieldRef<FlavorSynchronizer, ulong> LocalPlayerIdRef = AccessTools.FieldRefAccess<FlavorSynchronizer, ulong>("_localPlayerId");

	private static readonly FieldRef<FlavorSynchronizer, ulong> VanillaNextAllowedPingTimeRef = AccessTools.FieldRefAccess<FlavorSynchronizer, ulong>("_nextAllowedPingTime");

	private static readonly Dictionary<ulong, ulong> NextAllowedAtByPlayer = new Dictionary<ulong, ulong>();

	private static IReadOnlyList<MethodBase>? _targetMethods;

	private static bool Prepare()
	{
		if (FindTargetMethods().Count > 0)
		{
			return true;
		}
		MainFile.Logger.Warn("[MultiplayerPingSfx] Could not find multiplayer end-turn ping methods; skipping local ping sfx patch.", 1);
		return false;
	}

	private static IEnumerable<MethodBase> TargetMethods()
	{
		return FindTargetMethods();
	}

	private static IReadOnlyList<MethodBase> FindTargetMethods()
	{
		if (_targetMethods != null)
		{
			return _targetMethods;
		}
		_targetMethods = new MethodInfo[2]
		{
			AccessTools.Method(typeof(FlavorSynchronizer), "SendEndTurnPing", Type.EmptyTypes, (Type[])null),
			AccessTools.Method(typeof(FlavorSynchronizer), "HandleEndTurnPingMessage", new Type[2]
			{
				typeof(EndTurnPingMessage),
				typeof(ulong)
			}, (Type[])null)
		}.Where((MethodInfo method) => method != null).Cast<MethodBase>().ToArray();
		return _targetMethods;
	}

	private static void Prefix(FlavorSynchronizer __instance, MethodBase __originalMethod, out bool __state)
	{
		if (!ValencinaModConfig.EnableMultiplayerPingVoice)
		{
			__state = false;
		}
		else
		{
			__state = __originalMethod.Name != "SendEndTurnPing" || Time.GetTicksMsec() >= VanillaNextAllowedPingTimeRef.Invoke(__instance);
		}
	}

	private static void Postfix(FlavorSynchronizer __instance, MethodBase __originalMethod, object[] __args, bool __state)
	{
		if (!__state || !TryGetActualSender(__instance, __originalMethod, __args, out Player sender) || sender == null)
		{
			return;
		}
		Player val = sender;
		if (IsValencina(val))
		{
			ulong ticksMsec = Time.GetTicksMsec();
			ulong netId = val.NetId;
			if (!NextAllowedAtByPlayer.TryGetValue(netId, out var value) || ticksMsec >= value)
			{
				NextAllowedAtByPlayer[netId] = ticksMsec + 350;
				ValencinaLocalSfx.PlayMultiplayerPing((Node?)(object)NCombatRoom.Instance);
			}
		}
	}

	private static bool TryGetActualSender(FlavorSynchronizer synchronizer, MethodBase originalMethod, object[] args, out Player? sender)
	{
		sender = null;
		try
		{
			IPlayerCollection val = PlayerCollectionRef.Invoke(synchronizer);
			ulong num;
			if (originalMethod.Name == "SendEndTurnPing")
			{
				num = LocalPlayerIdRef.Invoke(synchronizer);
			}
			else
			{
				if (!(originalMethod.Name == "HandleEndTurnPingMessage") || args.Length < 2 || !(args[1] is ulong num2))
				{
					return false;
				}
				num = num2;
			}
			sender = val.GetPlayer(num);
			return sender != null;
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[MultiplayerPingSfx] Rejected ping because its sender could not be verified: " + ex.Message, 1);
			return false;
		}
	}

	private static bool IsValencina(Player? player)
	{
		if (!(((player != null) ? player.Character : null) is Valencina.ValencinaCode.Character.Valencina))
		{
			if (player == null)
			{
				return false;
			}
			return ((AbstractModel)player.Character).Id.Entry.Contains("VALENCINA", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}
}
