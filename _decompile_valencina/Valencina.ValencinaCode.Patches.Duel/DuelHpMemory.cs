using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rooms;

namespace Valencina.ValencinaCode.Patches.Duel;

internal static class DuelHpMemory
{
	private sealed class HpTable
	{
		public Dictionary<ulong, int> Values { get; } = new Dictionary<ulong, int>();
	}

	private static readonly ConditionalWeakTable<CombatRoom, HpTable> Tables = new ConditionalWeakTable<CombatRoom, HpTable>();

	internal static void SaveIfMissing(CombatRoom room, Player player, int hp)
	{
		Tables.GetOrCreateValue(room).Values.TryAdd(player.NetId, hp);
	}

	internal static void Restore(CombatRoom room)
	{
		if (!Tables.TryGetValue(room, out HpTable value))
		{
			return;
		}
		foreach (Player player in room.CombatState.Players)
		{
			if (value.Values.TryGetValue(player.NetId, out var value2))
			{
				player.Creature.SetCurrentHpInternal((decimal)Math.Clamp(value2, 0, player.Creature.MaxHp));
			}
		}
		Tables.Remove(room);
	}
}
