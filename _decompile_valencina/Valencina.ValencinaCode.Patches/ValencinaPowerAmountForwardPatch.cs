using System;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Powers;
using Valencina.ValencinaCode.Relics;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
internal static class ValencinaPowerAmountForwardPatch
{
	private static MethodBase? TargetMethod()
	{
		return AccessTools.Method(typeof(Hook), "AfterPowerAmountChanged", (Type[])null, (Type[])null);
	}

	[HarmonyPostfix]
	private static void AfterPowerAmountChangedPostfix(ref Task __result, object combatState, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		__result = ValencinaHookForwardHelpers.RunAfterOriginal(__result, async delegate
		{
			foreach (AbstractModel item in ValencinaHookForwardHelpers.IterateHookListeners(combatState))
			{
				CometKnifeRelic cometKnifeRelic = item as CometKnifeRelic;
				if (cometKnifeRelic == null)
				{
					OdinEyePower odinEyePower = item as OdinEyePower;
					if (odinEyePower == null)
					{
						ScorchMarkPower scorchMarkPower = item as ScorchMarkPower;
						if (scorchMarkPower == null)
						{
							DestinedFuturePower destinedFuturePower = item as DestinedFuturePower;
							if (destinedFuturePower == null)
							{
								FaceMyHatredPower faceMyHatredPower = item as FaceMyHatredPower;
								if (faceMyHatredPower == null)
								{
									AcceleratingMomentPower acceleratingMomentPower = item as AcceleratingMomentPower;
									if (acceleratingMomentPower != null)
									{
										await ValencinaHookForwardHelpers.RunModelContinuation(item, "after power amount changed", () => acceleratingMomentPower.ValencinaAfterPowerAmountChanged(power, amount, applier, cardSource));
									}
								}
								else
								{
									await ValencinaHookForwardHelpers.RunModelContinuation(item, "after power amount changed", () => faceMyHatredPower.ValencinaAfterPowerAmountChanged(power, amount, applier, cardSource));
								}
							}
							else
							{
								await ValencinaHookForwardHelpers.RunModelContinuation(item, "after power amount changed", () => destinedFuturePower.ValencinaAfterPowerAmountChanged(power, amount, applier, cardSource));
							}
						}
						else
						{
							await ValencinaHookForwardHelpers.RunModelContinuation(item, "after power amount changed", () => scorchMarkPower.ValencinaAfterPowerAmountChanged(power, amount, applier, cardSource));
						}
					}
					else
					{
						await ValencinaHookForwardHelpers.RunModelContinuation(item, "after power amount changed", () => odinEyePower.ValencinaAfterPowerAmountChanged(power, amount, applier, cardSource));
					}
				}
				else
				{
					await ValencinaHookForwardHelpers.RunModelContinuation(item, "after power amount changed", () => cometKnifeRelic.ValencinaAfterPowerAmountChanged((PlayerChoiceContext)new BlockingPlayerChoiceContext(), power, amount, applier, cardSource));
				}
			}
		});
	}
}
