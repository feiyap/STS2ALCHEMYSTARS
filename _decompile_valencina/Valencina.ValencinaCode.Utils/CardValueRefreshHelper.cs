using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Utils;

public static class CardValueRefreshHelper
{
	private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
	{
		public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();

		public new bool Equals(object? x, object? y)
		{
			return x == y;
		}

		public int GetHashCode(object obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}
	}

	public static async Task RefreshAsync(CardModel? sourceCard = null, Creature? owner = null)
	{
		_ = 6;
		try
		{
			HashSet<object> visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
			foreach (object state in EnumerateCandidateStates(sourceCard, owner))
			{
				if (state != null && visited.Add(state))
				{
					bool flag = false;
					bool flag2 = flag;
					flag = flag2 | await TryInvokeAsync(state, "RecalculateCardValues");
					flag2 = flag;
					flag = flag2 | await TryInvokeAsync(state, "RefreshCombatValues");
					flag2 = flag;
					flag = flag2 | await TryInvokeAsync(state, "RefreshValues");
					flag2 = flag;
					flag = flag2 | await TryInvokeAsync(state, "UpdateValues");
					flag2 = flag;
					if (flag2 | await TryInvokeAsync(state, "RecalculateValues"))
					{
						await RefreshCardsFromStateAsync(state);
						return;
					}
				}
			}
			foreach (CardModel item in EnumerateCandidateCards(sourceCard, owner))
			{
				await RefreshCardAsync(item);
			}
		}
		catch (Exception ex)
		{
			MainFile.Logger.Info("[CardValueRefreshHelper] refresh skipped: " + ex.Message, 1);
		}
	}

	public static Task RefreshAsync(object? root)
	{
		Creature owner = TryResolveCreature(root);
		return RefreshAsync((CardModel?)((root is CardModel) ? root : null), owner);
	}

	private static IEnumerable<object?> EnumerateCandidateStates(CardModel? sourceCard, Creature? owner)
	{
		object[] obj = new object[3] { sourceCard, null, null };
		obj[1] = ((sourceCard != null) ? sourceCard.Owner : null);
		obj[2] = owner;
		object?[] array = obj;
		foreach (object source in array)
		{
			foreach (object item in EnumerateMemberValues(source, "CombatState", "combatState", "PlayerCombatState", "playerCombatState", "State", "state"))
			{
				yield return item;
			}
		}
	}

	private static async Task RefreshCardsFromStateAsync(object state)
	{
		foreach (object item in EnumerateMemberValues(state, "Hand", "hand", "HandPile", "handPile", "CardsInHand", "cardsInHand", "DrawPile", "drawPile", "DiscardPile", "discardPile", "ExhaustPile", "exhaustPile", "PlayPile", "playPile", "CardsInCombat", "cardsInCombat"))
		{
			if (!(item is IEnumerable enumerable))
			{
				continue;
			}
			foreach (object item2 in enumerable)
			{
				CardModel val = (CardModel)((item2 is CardModel) ? item2 : null);
				if (val != null)
				{
					await RefreshCardAsync(val);
				}
			}
		}
	}

	private static IEnumerable<CardModel> EnumerateCandidateCards(CardModel? sourceCard, Creature? owner)
	{
		HashSet<object> visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
		if (sourceCard != null && visited.Add(sourceCard))
		{
			yield return sourceCard;
		}
		object[] array = new object[3];
		array[0] = ((sourceCard != null) ? sourceCard.Owner : null);
		array[1] = owner;
		array[2] = ((sourceCard != null) ? sourceCard.CombatState : null);
		object?[] array2 = array;
		foreach (object source in array2)
		{
			foreach (object item in EnumerateMemberValues(source, "Hand", "hand", "HandPile", "handPile", "CardsInHand", "cardsInHand", "DrawPile", "drawPile", "DiscardPile", "discardPile", "ExhaustPile", "exhaustPile", "PlayPile", "playPile", "Cards", "cards", "CardModels", "cardModels"))
			{
				if (!(item is IEnumerable enumerable))
				{
					continue;
				}
				foreach (object item2 in enumerable)
				{
					CardModel val = (CardModel)((item2 is CardModel) ? item2 : null);
					if (val != null && visited.Add(val))
					{
						yield return val;
					}
				}
			}
		}
	}

	private static async Task RefreshCardAsync(CardModel card)
	{
		await TryInvokeAsync(card, "RecalculateValues");
		await TryInvokeAsync(card, "RefreshValues");
		await TryInvokeAsync(card, "UpdateValues");
		await TryInvokeAsync(card, "UpdateDynamicVarPreview");
		await TryInvokeAsync(card, "UpdateVisuals");
		await TryInvokeAsync(card, "UpdateCardModel");
	}

	private static async Task<bool> TryInvokeAsync(object target, string methodName)
	{
		try
		{
			MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
			if (method == null)
			{
				return false;
			}
			if (method.Invoke(target, null) is Task task)
			{
				await task;
			}
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static IEnumerable<object?> EnumerateMemberValues(object? source, params string[] memberNames)
	{
		if (source == null)
		{
			yield break;
		}
		foreach (string name in memberNames)
		{
			object obj = null;
			try
			{
				obj = source.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(source);
			}
			catch
			{
			}
			if (obj != null)
			{
				yield return obj;
				continue;
			}
			try
			{
				obj = source.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(source);
			}
			catch
			{
			}
			if (obj != null)
			{
				yield return obj;
			}
		}
	}

	private static Creature? TryResolveCreature(object? root)
	{
		Creature val = (Creature)((root is Creature) ? root : null);
		if (val != null)
		{
			return val;
		}
		object[] array = new object[1] { root };
		foreach (object obj in array)
		{
			if (obj == null)
			{
				continue;
			}
			string[] array2 = new string[6] { "Owner", "owner", "Creature", "creature", "Target", "target" };
			foreach (string name in array2)
			{
				try
				{
					object? obj2 = obj.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(obj);
					Creature val2 = (Creature)((obj2 is Creature) ? obj2 : null);
					if (val2 != null)
					{
						return val2;
					}
				}
				catch
				{
				}
				try
				{
					object? obj4 = obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(obj);
					Creature val3 = (Creature)((obj4 is Creature) ? obj4 : null);
					if (val3 != null)
					{
						return val3;
					}
				}
				catch
				{
				}
			}
		}
		return null;
	}
}
