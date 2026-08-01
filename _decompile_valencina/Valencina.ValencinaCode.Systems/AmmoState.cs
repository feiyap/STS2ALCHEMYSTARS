using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace Valencina.ValencinaCode.Systems;

public static class AmmoState
{
	private sealed class State
	{
		public int MaxAmmo = 6;

		public int CurrentAmmo = 6;

		public int AmmoSpentThisCombat;

		public int AmmoSpentThisTurn;

		public void EnterCombat(int maxAmmo = 6)
		{
			MaxAmmo = ((maxAmmo <= 0) ? 6 : maxAmmo);
			CurrentAmmo = MaxAmmo;
			AmmoSpentThisCombat = 0;
			AmmoSpentThisTurn = 0;
		}

		public void ExitCombat()
		{
			CurrentAmmo = MaxAmmo;
			AmmoSpentThisCombat = 0;
			AmmoSpentThisTurn = 0;
		}
	}

	private sealed class ReferenceEqualityComparer : IEqualityComparer<Creature>
	{
		public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();

		public bool Equals(Creature? x, Creature? y)
		{
			return x == y;
		}

		public int GetHashCode(Creature obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}
	}

	public const int DefaultMaxAmmo = 6;

	private static readonly State FallbackState = new State();

	private static readonly Dictionary<ulong, State> StatesByPlayerNetId = new Dictionary<ulong, State>();

	private static readonly Dictionary<Creature, State> StatesByOwner = new Dictionary<Creature, State>(ReferenceEqualityComparer.Instance);

	public static int MaxAmmo => FallbackState.MaxAmmo;

	public static int CurrentAmmo => FallbackState.CurrentAmmo;

	public static int AmmoSpentThisCombat => FallbackState.AmmoSpentThisCombat;

	public static int AmmoSpentThisTurn => FallbackState.AmmoSpentThisTurn;

	public static void EnterCombat(int maxAmmo = 6)
	{
		FallbackState.EnterCombat(maxAmmo);
		StatesByPlayerNetId.Clear();
		StatesByOwner.Clear();
	}

	public static void ExitCombat()
	{
		FallbackState.ExitCombat();
		StatesByPlayerNetId.Clear();
		StatesByOwner.Clear();
	}

	public static void EnsureOwner(Creature? owner, int maxAmmo = 6)
	{
		if (owner != null)
		{
			StateFor(owner, maxAmmo);
		}
	}

	public static void SyncOwner(Creature? owner, int currentAmmo, int maxAmmo)
	{
		if (owner != null)
		{
			if (maxAmmo <= 0)
			{
				maxAmmo = 6;
			}
			State state = StateFor(owner, maxAmmo);
			state.MaxAmmo = maxAmmo;
			state.CurrentAmmo = Math.Clamp(currentAmmo, 0, state.MaxAmmo);
		}
	}

	public static int GetMaxAmmo(Creature? owner)
	{
		return StateFor(owner).MaxAmmo;
	}

	public static int GetCurrentAmmo(Creature? owner)
	{
		return StateFor(owner).CurrentAmmo;
	}

	public static int GetAmmoSpentThisCombat(Creature? owner)
	{
		return StateFor(owner).AmmoSpentThisCombat;
	}

	public static int GetAmmoSpentThisTurn(Creature? owner)
	{
		return StateFor(owner).AmmoSpentThisTurn;
	}

	public static void StartPlayerTurn(Creature? owner)
	{
		StateFor(owner).AmmoSpentThisTurn = 0;
	}

	public static int Add(Creature? owner, int amount)
	{
		if (amount <= 0)
		{
			return 0;
		}
		State state = StateFor(owner);
		int currentAmmo = state.CurrentAmmo;
		state.CurrentAmmo = Math.Min(state.MaxAmmo, state.CurrentAmmo + amount);
		return state.CurrentAmmo - currentAmmo;
	}

	public static int IncreaseMaxAmmo(Creature? owner, int amount)
	{
		if (amount <= 0)
		{
			return 0;
		}
		State state = StateFor(owner);
		int maxAmmo = state.MaxAmmo;
		state.MaxAmmo += amount;
		return state.MaxAmmo - maxAmmo;
	}

	public static int ReloadToFull(Creature? owner)
	{
		State state = StateFor(owner);
		int currentAmmo = state.CurrentAmmo;
		state.CurrentAmmo = state.MaxAmmo;
		return state.CurrentAmmo - currentAmmo;
	}

	public static int TryConsume(Creature? owner, int request)
	{
		if (request <= 0)
		{
			return 0;
		}
		State state = StateFor(owner);
		int num = Math.Min(request, Math.Max(0, state.CurrentAmmo));
		if (num <= 0)
		{
			return 0;
		}
		state.CurrentAmmo -= num;
		state.AmmoSpentThisCombat += num;
		state.AmmoSpentThisTurn += num;
		return num;
	}

	public static string DisplayText(Creature? owner = null)
	{
		State state = StateFor(owner);
		return $"Ammo {state.CurrentAmmo}/{state.MaxAmmo}";
	}

	private static State StateFor(Creature? owner, int maxAmmo = 6)
	{
		if (owner == null)
		{
			return FallbackState;
		}
		if (owner.Player != null)
		{
			return StateForPlayer(owner.Player.NetId, maxAmmo);
		}
		if (!StatesByOwner.TryGetValue(owner, out State value))
		{
			value = new State();
			value.EnterCombat(maxAmmo);
			StatesByOwner[owner] = value;
		}
		return value;
	}

	private static State StateForPlayer(ulong netId, int maxAmmo)
	{
		if (!StatesByPlayerNetId.TryGetValue(netId, out State value))
		{
			value = new State();
			value.EnterCombat(maxAmmo);
			StatesByPlayerNetId[netId] = value;
		}
		return value;
	}
}
