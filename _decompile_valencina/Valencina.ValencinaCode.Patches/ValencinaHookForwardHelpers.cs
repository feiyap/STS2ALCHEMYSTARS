using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Patches;

internal static class ValencinaHookForwardHelpers
{
	internal static IEnumerable<AbstractModel> IterateHookListeners(object? combatState)
	{
		CombatState val = (CombatState)((combatState is CombatState) ? combatState : null);
		if (val != null)
		{
			return val.IterateHookListeners();
		}
		if (combatState == null)
		{
			return Array.Empty<AbstractModel>();
		}
		if (combatState.GetType().GetMethod("IterateHookListeners", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(combatState, null) is IEnumerable<AbstractModel> result)
		{
			return result;
		}
		MainFile.Logger.Warn("[ValencinaHookForward] Could not enumerate hook listeners for combat state type " + combatState.GetType().FullName + ".", 1);
		return Array.Empty<AbstractModel>();
	}

	internal static async Task RunAfterOriginal(Task original, Func<Task> continuation)
	{
		try
		{
			await original;
		}
		catch (Exception value)
		{
			MainFile.Logger.Warn($"[ValencinaHookForward] original hook failed; continuing Valencina hook guard: {value}", 1);
		}
		try
		{
			await continuation();
		}
		catch (Exception value2)
		{
			MainFile.Logger.Warn($"[ValencinaHookForward] continuation failed: {value2}", 1);
		}
	}

	internal static async Task RunModelContinuation(AbstractModel model, string operation, Func<Task> continuation)
	{
		try
		{
			await continuation();
		}
		catch (Exception value)
		{
			MainFile.Logger.Warn($"[ValencinaHookForward] {operation} failed for {((object)model).GetType().Name}: {value}", 1);
		}
		finally
		{
			SafeInvokeExecutionFinished(model, operation);
		}
	}

	internal static void SafeInvokeExecutionFinished(AbstractModel model, string operation)
	{
		try
		{
			model.InvokeExecutionFinished();
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn($"[ValencinaHookForward] InvokeExecutionFinished failed after {operation} for {((object)model).GetType().Name}: {ex.Message}", 1);
		}
	}

	internal static async Task InvokeLegacyTurnHookIfPresent(AbstractModel model, string methodName, PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature>? creatures = null)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		MethodInfo sideTurnMethod = AccessTools.Method(((object)model).GetType(), methodName, new Type[3]
		{
			typeof(PlayerChoiceContext),
			typeof(CombatSide),
			typeof(IEnumerable<Creature>)
		}, (Type[])null);
		if (ShouldForwardLegacyHook(model, sideTurnMethod))
		{
			await RunModelContinuation(model, methodName, async delegate
			{
				if (sideTurnMethod.Invoke(model, new object[3]
				{
					choiceContext,
					side,
					creatures ?? Enumerable.Empty<Creature>()
				}) is Task task)
				{
					await task;
				}
			});
			return;
		}
		MethodInfo method = AccessTools.Method(((object)model).GetType(), methodName, new Type[2]
		{
			typeof(PlayerChoiceContext),
			typeof(CombatSide)
		}, (Type[])null);
		if (!ShouldForwardLegacyHook(model, method))
		{
			return;
		}
		await RunModelContinuation(model, methodName, async delegate
		{
			if (method.Invoke(model, new object[2] { choiceContext, side }) is Task task)
			{
				await task;
			}
		});
	}

	internal static bool TryExtractTurnHookArgs(object[] args, out object? combatState, out PlayerChoiceContext? choiceContext, out CombatSide side, out IEnumerable<Creature>? creatures)
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected I4, but got Unknown
		combatState = null;
		choiceContext = null;
		side = (CombatSide)0;
		creatures = null;
		bool flag = false;
		foreach (object obj in args)
		{
			PlayerChoiceContext val = (PlayerChoiceContext)((obj is PlayerChoiceContext) ? obj : null);
			if (val != null)
			{
				if (choiceContext == null)
				{
					choiceContext = val;
				}
			}
			else if (obj is IEnumerable<Creature> enumerable)
			{
				if (creatures == null)
				{
					creatures = enumerable;
				}
			}
			else if (obj is CombatSide val2)
			{
				side = (CombatSide)(int)val2;
				flag = true;
			}
			else if (obj is CombatState)
			{
				if (combatState == null)
				{
					combatState = obj;
				}
			}
			else if (obj != null && HasHookListeners(obj) && combatState == null)
			{
				combatState = obj;
			}
		}
		if (choiceContext == null)
		{
			choiceContext = (PlayerChoiceContext?)new BlockingPlayerChoiceContext();
		}
		return combatState != null && flag;
	}

	private static bool HasHookListeners(object value)
	{
		return value.GetType().GetMethod("IterateHookListeners", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null;
	}

	private static bool ShouldForwardLegacyHook(AbstractModel model, MethodInfo? method)
	{
		if (method == null)
		{
			return false;
		}
		if (((object)model).GetType().Assembly != typeof(MainFile).Assembly)
		{
			return false;
		}
		try
		{
			return method.GetBaseDefinition().DeclaringType == method.DeclaringType;
		}
		catch
		{
			return true;
		}
	}
}
