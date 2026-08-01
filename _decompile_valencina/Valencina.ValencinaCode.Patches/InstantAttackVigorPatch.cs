using System;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(VigorPower), "AfterAttack")]
internal static class InstantAttackVigorPatch
{
	private static readonly MethodInfo? GetInternalDataMethod = typeof(VigorPower).GetMethod("GetInternalData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

	private static readonly Type? DataType = typeof(VigorPower).GetNestedType("Data", BindingFlags.NonPublic);

	private static readonly FieldInfo? CommandToModifyField = DataType?.GetField("commandToModify", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

	private static readonly FieldInfo? AmountWhenAttackStartedField = DataType?.GetField("amountWhenAttackStarted", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

	private static bool Prefix(VigorPower __instance, AttackCommand command, ref Task __result)
	{
		if (command.Attacker != ((PowerModel)__instance).Owner || (!InstantAttackBreathingMethodRegistry.TryGet(((PowerModel)__instance).Owner, out var _) && !ValencinaAttackScope.ShouldSuppressBreathingMethodAfterAttack(((PowerModel)__instance).Owner)))
		{
			return true;
		}
		ClearVigorAttackState(__instance);
		__result = Task.CompletedTask;
		return false;
	}

	private static void ClearVigorAttackState(VigorPower power)
	{
		if (!(GetInternalDataMethod == null) && !(DataType == null))
		{
			object obj = GetInternalDataMethod.MakeGenericMethod(DataType).Invoke(power, null);
			if (obj != null)
			{
				CommandToModifyField?.SetValue(obj, null);
				AmountWhenAttackStartedField?.SetValue(obj, 0);
			}
		}
	}
}
