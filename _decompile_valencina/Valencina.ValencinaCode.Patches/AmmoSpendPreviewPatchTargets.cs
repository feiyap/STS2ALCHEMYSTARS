using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace Valencina.ValencinaCode.Patches;

internal static class AmmoSpendPreviewPatchTargets
{
	private static readonly HashSet<string> LoggedMissingMethods = new HashSet<string>(StringComparer.Ordinal);

	public static MethodBase? NCardMethod(string methodName, params Type[] argumentTypes)
	{
		MethodInfo obj = ((argumentTypes.Length == 0) ? AccessTools.Method(typeof(NCard), methodName, (Type[])null, (Type[])null) : AccessTools.Method(typeof(NCard), methodName, argumentTypes, (Type[])null));
		if ((MethodBase?)obj == (MethodBase?)null && LoggedMissingMethods.Add(methodName))
		{
			MainFile.Logger.Info("[AmmoSpendPreviewPatch] NCard." + methodName + " not found in current sts2.dll; skipping this optional card-corner refresh hook.", 1);
		}
		return obj;
	}
}
