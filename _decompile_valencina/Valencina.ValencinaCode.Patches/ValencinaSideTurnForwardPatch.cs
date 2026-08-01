using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Monsters;
using Valencina.ValencinaCode.Powers;
using Valencina.ValencinaCode.Relics.Rien;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
internal static class ValencinaSideTurnForwardPatch
{
	private static MethodBase? TargetMethod()
	{
		return AccessTools.Method(typeof(Hook), "AfterSideTurnStart", (Type[])null, (Type[])null);
	}

	[HarmonyPostfix]
	private static void AfterSideTurnStartPostfix(ref Task __result, ICombatState combatState, CombatSide side, IReadOnlyList<Creature> participants)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		__result = ValencinaHookForwardHelpers.RunAfterOriginal(__result, async delegate
		{
			foreach (AbstractModel item in ValencinaHookForwardHelpers.IterateHookListeners(combatState))
			{
				Rainstorm rainstorm = item as Rainstorm;
				if (rainstorm == null)
				{
					UngezieferKaiser ungezieferKaiser = item as UngezieferKaiser;
					if (ungezieferKaiser == null)
					{
						EmperorExcisionPower emperorExcisionPower = item as EmperorExcisionPower;
						if (emperorExcisionPower == null)
						{
							Reverberation reverberation = item as Reverberation;
							if (reverberation != null)
							{
								await ValencinaHookForwardHelpers.RunModelContinuation(item, "after side turn start", () => reverberation.ValencinaAfterSideTurnStart(side, participants, combatState));
							}
						}
						else
						{
							await ValencinaHookForwardHelpers.RunModelContinuation(item, "after side turn start legacy", () => ((AbstractModel)emperorExcisionPower).AfterSideTurnStart(side, participants, combatState));
						}
					}
					else
					{
						await ValencinaHookForwardHelpers.RunModelContinuation(item, "after side turn start", () => ungezieferKaiser.ValencinaAfterSideTurnStart(side, participants, combatState));
					}
				}
				else
				{
					await ValencinaHookForwardHelpers.RunModelContinuation(item, "after side turn start", () => rainstorm.ValencinaAfterSideTurnStart(side, participants, combatState));
				}
			}
		});
	}
}
