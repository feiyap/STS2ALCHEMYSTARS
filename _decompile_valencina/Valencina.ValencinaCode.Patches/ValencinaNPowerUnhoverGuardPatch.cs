using System;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
internal static class ValencinaNPowerUnhoverGuardPatch
{
	private static readonly FieldInfo? ModelField = AccessTools.Field(typeof(NPower), "_model");

	private static readonly FieldInfo? IconField = AccessTools.Field(typeof(NPower), "_icon");

	private static MethodBase? TargetMethod()
	{
		MethodInfo methodInfo = AccessTools.Method(typeof(NPower), "OnUnhovered", (Type[])null, (Type[])null);
		if ((MethodBase?)methodInfo == (MethodBase?)null)
		{
			ValencinaProbeLog.Warn("unhover-target-missing", "NPower.OnUnhovered patch target was not found; stale unhover crashes may still appear.", 1);
			return methodInfo;
		}
		ValencinaProbeLog.Info("unhover-target-ok", "NPower.OnUnhovered guard patch target resolved.", 1);
		return methodInfo;
	}

	private static bool Prefix(NPower __instance)
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)__instance))
			{
				object? obj = ModelField?.GetValue(__instance);
				PowerModel val = (PowerModel)((obj is PowerModel) ? obj : null);
				if (val != null && val.Owner != null && NCombatRoom.Instance != null && GodotObject.IsInstanceValid((GodotObject)(object)NCombatRoom.Instance))
				{
					NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(val.Owner);
					if (creatureNode != null && GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
					{
						creatureNode.HideHoverTips();
					}
				}
			}
			object? obj2 = IconField?.GetValue(__instance);
			TextureRect val2 = (TextureRect)((obj2 is TextureRect) ? obj2 : null);
			if (val2 != null && GodotObject.IsInstanceValid((GodotObject)(object)val2))
			{
				((Control)val2).Scale = Vector2.One;
			}
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[NPowerHoverGuard] Suppressed stale power unhover: " + ex.Message, 1);
		}
		return false;
	}
}
