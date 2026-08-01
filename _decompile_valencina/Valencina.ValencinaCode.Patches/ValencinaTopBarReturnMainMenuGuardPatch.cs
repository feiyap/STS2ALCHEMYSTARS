using System;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
internal static class ValencinaTopBarReturnMainMenuGuardPatch
{
	private static MethodBase? TargetMethod()
	{
		return AccessTools.Method(typeof(NTopBar), "ToggleAnimState", new Type[1] { typeof(Node) }, (Type[])null);
	}

	private static bool Prefix(NTopBar __instance)
	{
		if (!IsLiveInTree((Node?)(object)__instance))
		{
			return false;
		}
		if (!IsLiveInTree((Node?)(object)__instance.Pause))
		{
			return false;
		}
		if (!IsLiveInTree((Node?)(object)__instance.Deck))
		{
			return false;
		}
		return true;
	}

	private static bool IsLiveInTree(Node? node)
	{
		if (node != null && GodotObject.IsInstanceValid((GodotObject)(object)node) && !((GodotObject)node).IsQueuedForDeletion())
		{
			return node.IsInsideTree();
		}
		return false;
	}
}
