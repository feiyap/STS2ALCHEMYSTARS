using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.addons.mega_text;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NTopBar), "Initialize")]
internal static class ValencinaWarDifficultyTopBarPatch
{
	private static void Postfix(NTopBar __instance, IRunState runState)
	{
		if (ValencinaWarDifficulty.IsActive(runState))
		{
			MegaLabel nodeOrNull = ((Node)__instance).GetNodeOrNull<MegaLabel>(NodePath.op_Implicit("%AscensionLabel"));
			if (nodeOrNull != null)
			{
				nodeOrNull.SetTextAutoSize("?");
			}
		}
	}
}
