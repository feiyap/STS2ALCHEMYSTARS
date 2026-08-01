using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Powers;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
internal static class ValencinaAfterAttackForwardPatch
{
	private static MethodBase? TargetMethod()
	{
		return AccessTools.Method(typeof(Hook), "AfterAttack", (Type[])null, (Type[])null);
	}

	[HarmonyPostfix]
	private static void Postfix(ref Task __result, object combatState, AttackCommand command)
	{
		__result = ValencinaHookForwardHelpers.RunAfterOriginal(__result, async delegate
		{
			List<InstantForesightPower> foresightPowers = ValencinaHookForwardHelpers.IterateHookListeners(combatState).OfType<InstantForesightPower>().Distinct()
				.ToList();
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(59, 4);
			defaultInterpolatedStringHandler.AppendLiteral("AfterAttack scan powers=");
			defaultInterpolatedStringHandler.AppendFormatted(foresightPowers.Count);
			defaultInterpolatedStringHandler.AppendLiteral(" attacker=");
			Creature attacker = command.Attacker;
			defaultInterpolatedStringHandler.AppendFormatted(((attacker != null) ? attacker.Name : null) ?? "null");
			defaultInterpolatedStringHandler.AppendLiteral(" targetSide=");
			defaultInterpolatedStringHandler.AppendFormatted<CombatSide>(command.TargetSide);
			defaultInterpolatedStringHandler.AppendLiteral(" combatState=");
			defaultInterpolatedStringHandler.AppendFormatted(combatState?.GetType().Name ?? "null");
			ValencinaProbeLog.Info("precog-afterattack-scan", defaultInterpolatedStringHandler.ToStringAndClear(), 20);
			List<(InstantForesightPower Power, InstantForesightPower.PreparedDodgeCounter Counter)> preparedCounters = new List<(InstantForesightPower, InstantForesightPower.PreparedDodgeCounter)>();
			try
			{
				foreach (InstantForesightPower foresight in foresightPowers)
				{
					try
					{
						InstantForesightPower.PreparedDodgeCounter? preparedDodgeCounter = await foresight.PrepareDodgeCounterAfterAttackAsync(command);
						if (preparedDodgeCounter.HasValue)
						{
							preparedCounters.Add((foresight, preparedDodgeCounter.Value));
							defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(49, 4);
							defaultInterpolatedStringHandler.AppendLiteral("Prepared counter owner=");
							Creature owner = ((PowerModel)foresight).Owner;
							defaultInterpolatedStringHandler.AppendFormatted(((owner != null) ? owner.Name : null) ?? "null");
							defaultInterpolatedStringHandler.AppendLiteral("/net=");
							Creature owner2 = ((PowerModel)foresight).Owner;
							object obj;
							if (owner2 == null)
							{
								obj = null;
							}
							else
							{
								Player player = owner2.Player;
								obj = ((player != null) ? player.NetId.ToString() : null);
							}
							if (obj == null)
							{
								obj = "null";
							}
							defaultInterpolatedStringHandler.AppendFormatted((string?)obj);
							defaultInterpolatedStringHandler.AppendLiteral(" attacker=");
							defaultInterpolatedStringHandler.AppendFormatted(preparedDodgeCounter.Value.Attacker.Name);
							defaultInterpolatedStringHandler.AppendLiteral(" prevented=");
							defaultInterpolatedStringHandler.AppendFormatted(preparedDodgeCounter.Value.PreventedDamage);
							ValencinaProbeLog.Info("precog-afterattack-prepared", defaultInterpolatedStringHandler.ToStringAndClear(), 30);
						}
					}
					catch (Exception ex)
					{
						defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(53, 5);
						defaultInterpolatedStringHandler.AppendLiteral("Counter prepare failed owner=");
						Creature owner3 = ((PowerModel)foresight).Owner;
						defaultInterpolatedStringHandler.AppendFormatted(((owner3 != null) ? owner3.Name : null) ?? "null");
						defaultInterpolatedStringHandler.AppendLiteral("/net=");
						Creature owner4 = ((PowerModel)foresight).Owner;
						object obj2;
						if (owner4 == null)
						{
							obj2 = null;
						}
						else
						{
							Player player2 = owner4.Player;
							obj2 = ((player2 != null) ? player2.NetId.ToString() : null);
						}
						if (obj2 == null)
						{
							obj2 = "null";
						}
						defaultInterpolatedStringHandler.AppendFormatted((string?)obj2);
						defaultInterpolatedStringHandler.AppendLiteral(" attacker=");
						Creature attacker2 = command.Attacker;
						defaultInterpolatedStringHandler.AppendFormatted(((attacker2 != null) ? attacker2.Name : null) ?? "null");
						defaultInterpolatedStringHandler.AppendLiteral(" error=");
						defaultInterpolatedStringHandler.AppendFormatted(ex.GetType().Name);
						defaultInterpolatedStringHandler.AppendLiteral(": ");
						defaultInterpolatedStringHandler.AppendFormatted(ex.Message);
						ValencinaProbeLog.Warn("precog-afterattack-prepare-error", defaultInterpolatedStringHandler.ToStringAndClear(), 20);
					}
				}
				if (preparedCounters.Count > 0)
				{
					Logger logger = MainFile.Logger;
					defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(63, 3);
					defaultInterpolatedStringHandler.AppendLiteral("[Precognition] prepared dodge counters count=");
					defaultInterpolatedStringHandler.AppendFormatted(preparedCounters.Count);
					defaultInterpolatedStringHandler.AppendLiteral(" powers=");
					defaultInterpolatedStringHandler.AppendFormatted(foresightPowers.Count);
					defaultInterpolatedStringHandler.AppendLiteral(" attacker=");
					Creature attacker3 = command.Attacker;
					defaultInterpolatedStringHandler.AppendFormatted(((attacker3 != null) ? attacker3.Name : null) ?? "null");
					logger.Info(defaultInterpolatedStringHandler.ToStringAndClear(), 1);
					await Cmd.CustomScaledWait(0.025f, 0.025f, false, default(CancellationToken));
				}
				foreach (var (foresight, prepared) in preparedCounters)
				{
					try
					{
						await foresight.TriggerPreparedDodgeCounterAsync(prepared);
					}
					catch (Exception ex2)
					{
						defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(53, 5);
						defaultInterpolatedStringHandler.AppendLiteral("Counter trigger failed owner=");
						Creature owner5 = ((PowerModel)foresight).Owner;
						defaultInterpolatedStringHandler.AppendFormatted(((owner5 != null) ? owner5.Name : null) ?? "null");
						defaultInterpolatedStringHandler.AppendLiteral("/net=");
						Creature owner6 = ((PowerModel)foresight).Owner;
						object obj3;
						if (owner6 == null)
						{
							obj3 = null;
						}
						else
						{
							Player player3 = owner6.Player;
							obj3 = ((player3 != null) ? player3.NetId.ToString() : null);
						}
						if (obj3 == null)
						{
							obj3 = "null";
						}
						defaultInterpolatedStringHandler.AppendFormatted((string?)obj3);
						defaultInterpolatedStringHandler.AppendLiteral(" attacker=");
						defaultInterpolatedStringHandler.AppendFormatted(prepared.Attacker.Name);
						defaultInterpolatedStringHandler.AppendLiteral(" error=");
						defaultInterpolatedStringHandler.AppendFormatted(ex2.GetType().Name);
						defaultInterpolatedStringHandler.AppendLiteral(": ");
						defaultInterpolatedStringHandler.AppendFormatted(ex2.Message);
						ValencinaProbeLog.Warn("precog-afterattack-trigger-error", defaultInterpolatedStringHandler.ToStringAndClear(), 20);
					}
				}
			}
			finally
			{
				foreach (InstantForesightPower item in foresightPowers)
				{
					ValencinaHookForwardHelpers.SafeInvokeExecutionFinished((AbstractModel)(object)item, "after attack");
				}
			}
		});
	}
}
