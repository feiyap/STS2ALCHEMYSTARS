using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
internal static class ValencinaWarAmbushStartBannerDurationPatch
{
	private const double AmbushTitleHoldSeconds = 2.55;

	private static MethodBase TargetMethod()
	{
		return AccessTools.AsyncMoveNext((MethodBase)AccessTools.Method(typeof(NCombatStartBanner), "AnimateVfx", (Type[])null, (Type[])null));
	}

	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		List<CodeInstruction> codes = instructions.ToList();
		MethodInfo adjustDelay = AccessTools.Method(typeof(ValencinaWarAmbushStartBannerDurationPatch), "AdjustTitleHold", (Type[])null, (Type[])null);
		bool adjusted = false;
		for (int index = 0; index < codes.Count; index++)
		{
			CodeInstruction instruction = codes[index];
			yield return instruction;
			if (index + 1 < codes.Count && instruction.opcode == OpCodes.Ldc_R8 && instruction.operand is double num && Math.Abs(num - 1.2999999523162842) < 0.001 && codes[index + 1].operand is MethodInfo { Name: "SetDelay" })
			{
				yield return new CodeInstruction(OpCodes.Call, (object)adjustDelay);
				adjusted = true;
			}
		}
		if (!adjusted)
		{
			MainFile.Logger.Error("[SmogWarAmbush] Could not locate the combat-start title hold delay; the ambush banner uses vanilla timing.", 1);
		}
	}

	private static double AdjustTitleHold(double vanillaSeconds)
	{
		RunState obj = RunManager.Instance.DebugOnlyGetState();
		AbstractRoom obj2 = ((obj != null) ? ((IRunState)obj).CurrentRoom : null);
		CombatRoom val = (CombatRoom)(object)((obj2 is CombatRoom) ? obj2 : null);
		if (val == null || !ValencinaWarAmbushEntryPatch.IsWarAmbushEncounter(val.Encounter))
		{
			return vanillaSeconds;
		}
		return 2.55;
	}
}
