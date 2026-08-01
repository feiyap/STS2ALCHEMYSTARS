using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
internal static class ValencinaTurnHookFaultGuardPatch
{
	private static IEnumerable<MethodBase> TargetMethods()
	{
		string[] array = new string[5] { "BeforeTurnEnd", "AfterTurnEnd", "AfterTurnEndLate", "AfterSideTurnEnd", "AfterSideTurnEndLate" };
		foreach (string text in array)
		{
			MethodInfo methodInfo = AccessTools.Method(typeof(Hook), text, (Type[])null, (Type[])null);
			if (methodInfo != null)
			{
				yield return methodInfo;
			}
		}
	}

	[HarmonyPostfix]
	private static void Postfix(ref Task __result, MethodBase __originalMethod, object[] __args)
	{
		__result = ValencinaHookForwardHelpers.RunAfterOriginal(__result, async delegate
		{
			if (ValencinaHookForwardHelpers.TryExtractTurnHookArgs(__args, out object combatState, out PlayerChoiceContext choiceContext, out CombatSide side, out IEnumerable<Creature> creatures))
			{
				foreach (AbstractModel item in ValencinaHookForwardHelpers.IterateHookListeners(combatState))
				{
					await ValencinaHookForwardHelpers.InvokeLegacyTurnHookIfPresent(item, __originalMethod.Name, choiceContext, side, creatures);
				}
			}
		});
	}
}
