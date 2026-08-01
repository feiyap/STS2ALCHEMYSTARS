using System;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Relics;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
internal static class ValencinaAutoPrePlayPhaseForwardPatch
{
	private static MethodBase? TargetMethod()
	{
		return AccessTools.Method(typeof(Hook), "AfterAutoPrePlayPhaseEntered", (Type[])null, (Type[])null) ?? AccessTools.Method(typeof(Hook), "BeforePlayPhaseStart", (Type[])null, (Type[])null);
	}

	[HarmonyPostfix]
	private static void AfterAutoPrePlayPhaseEnteredPostfix(ref Task __result, HookPlayerChoiceContext playerChoiceContext, object combatState, Player player)
	{
		__result = ValencinaHookForwardHelpers.RunAfterOriginal(__result, async delegate
		{
			foreach (AbstractModel model in ValencinaHookForwardHelpers.IterateHookListeners(combatState))
			{
				AryaVijnanaRelic relic = model as AryaVijnanaRelic;
				if (relic != null)
				{
					await ValencinaHookForwardHelpers.RunModelContinuation(model, "before play phase", async delegate
					{
						((PlayerChoiceContext)playerChoiceContext).PushModel(model);
						try
						{
							await relic.ValencinaBeforePlayPhaseStart((PlayerChoiceContext)(object)playerChoiceContext, player);
						}
						finally
						{
							((PlayerChoiceContext)playerChoiceContext).PopModel(model);
						}
					});
				}
			}
		});
	}
}
