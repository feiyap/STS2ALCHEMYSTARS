using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace Valencina.ValencinaCode.Utils;

public static class InstantAttackBreathingMethodRegistry
{
	private sealed class PreserveFrame(int amount, bool clearWhenCardPlayEnds)
	{
		public int Amount { get; } = amount;

		public bool ClearWhenCardPlayEnds { get; } = clearWhenCardPlayEnds;
	}

	private static readonly object Gate = new object();

	private static readonly Dictionary<object, Stack<PreserveFrame>> ActivePreserves = new Dictionary<object, Stack<PreserveFrame>>(ReferenceEqualityComparer.Instance);

	public static void Begin(Creature? owner, int amount)
	{
		Begin(owner, amount, clearWhenCardPlayEnds: false);
	}

	public static void BeginForCardPlay(Creature? owner, int amount)
	{
		Begin(owner, amount, clearWhenCardPlayEnds: true);
	}

	private static void Begin(Creature? owner, int amount, bool clearWhenCardPlayEnds)
	{
		if (owner == null || amount <= 0)
		{
			return;
		}
		lock (Gate)
		{
			if (!ActivePreserves.TryGetValue(owner, out Stack<PreserveFrame> value))
			{
				value = new Stack<PreserveFrame>();
				ActivePreserves[owner] = value;
			}
			value.Push(new PreserveFrame(amount, clearWhenCardPlayEnds));
		}
	}

	public static void End(Creature? owner)
	{
		if (owner == null)
		{
			return;
		}
		lock (Gate)
		{
			if (ActivePreserves.TryGetValue(owner, out Stack<PreserveFrame> value))
			{
				if (value.Count > 0)
				{
					value.Pop();
				}
				if (value.Count == 0)
				{
					ActivePreserves.Remove(owner);
				}
			}
		}
	}

	public static bool TryGet(Creature? owner, out int amount)
	{
		if (owner == null)
		{
			amount = 0;
			return false;
		}
		lock (Gate)
		{
			if (ActivePreserves.TryGetValue(owner, out Stack<PreserveFrame> value) && value.Count > 0)
			{
				amount = value.Peek().Amount;
				return true;
			}
		}
		amount = 0;
		return false;
	}

	public static void EndCardPlayWindows()
	{
		lock (Gate)
		{
			List<object> list = null;
			foreach (KeyValuePair<object, Stack<PreserveFrame>> activePreserf in ActivePreserves)
			{
				Stack<PreserveFrame> value = activePreserf.Value;
				while (value.Count > 0 && value.Peek().ClearWhenCardPlayEnds)
				{
					value.Pop();
				}
				if (value.Count == 0)
				{
					(list ?? (list = new List<object>())).Add(activePreserf.Key);
				}
			}
			if (list == null)
			{
				return;
			}
			foreach (object item in list)
			{
				ActivePreserves.Remove(item);
			}
		}
	}

	public static void Clear(Creature? owner)
	{
		if (owner == null)
		{
			return;
		}
		lock (Gate)
		{
			ActivePreserves.Remove(owner);
		}
	}

	public static void ClearAll()
	{
		lock (Gate)
		{
			ActivePreserves.Clear();
		}
	}

	public static void Register(Creature? owner, int amount)
	{
		Begin(owner, amount);
	}

	public static bool TryTake(Creature? owner, out int amount)
	{
		if (!TryGet(owner, out amount))
		{
			return false;
		}
		End(owner);
		return true;
	}
}
