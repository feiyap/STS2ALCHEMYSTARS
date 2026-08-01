using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
internal static class ValencinaNPowerHoverGuardPatch
{
	private static readonly FieldInfo? ModelField = AccessTools.Field(typeof(NPower), "_model");

	private static readonly FieldInfo? IconField = AccessTools.Field(typeof(NPower), "_icon");

	private static MethodBase? TargetMethod()
	{
		MethodInfo methodInfo = AccessTools.Method(typeof(NPower), "OnHovered", (Type[])null, (Type[])null);
		if ((MethodBase?)methodInfo == (MethodBase?)null)
		{
			ValencinaProbeLog.Warn("hover-target-missing", "NPower.OnHovered patch target was not found; stale hover crashes may still appear.", 1);
			return methodInfo;
		}
		ValencinaProbeLog.Info("hover-target-ok", "NPower.OnHovered guard patch target resolved.", 1);
		return methodInfo;
	}

	private static bool Prefix(NPower __instance)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!TryReadSafeHoverState(__instance, out PowerModel _, out TextureRect icon, out NCreature ownerNode, out List<IHoverTip> tips))
			{
				return false;
			}
			if (ownerNode == null || tips == null)
			{
				return false;
			}
			ownerNode.ShowHoverTips((IEnumerable<IHoverTip>)tips);
			if (icon != null && GodotObject.IsInstanceValid((GodotObject)(object)icon))
			{
				((Control)icon).Scale = Vector2.One * 1.1f;
			}
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[NPowerHoverGuard] Suppressed stale power hover: " + ex.Message, 1);
		}
		return false;
	}

	internal static bool TryReadSafeHoverState(NPower instance, out PowerModel? model, out TextureRect? icon, out NCreature? ownerNode, out List<IHoverTip>? tips)
	{
		model = null;
		icon = null;
		ownerNode = null;
		tips = null;
		if (!GodotObject.IsInstanceValid((GodotObject)(object)instance))
		{
			ValencinaProbeLog.Info("hover-skip-invalid-instance", "Skipped power hover: NPower instance invalid.");
			return false;
		}
		object? obj = ModelField?.GetValue(instance);
		model = (PowerModel?)((obj is PowerModel) ? obj : null);
		PowerModel? obj2 = model;
		if (((obj2 != null) ? obj2.Owner : null) == null)
		{
			ValencinaProbeLog.Info("hover-skip-missing-owner", "Skipped power hover: model or owner missing.");
			return false;
		}
		Node instance2 = (Node)(object)NCombatRoom.Instance;
		if (instance2 == null || !GodotObject.IsInstanceValid((GodotObject)(object)instance2))
		{
			ValencinaProbeLog.Info("hover-skip-missing-combat-room", "Skipped power hover for " + ((AbstractModel)model).Id.Entry + ": combat room unavailable.");
			return false;
		}
		NCombatRoom instance3 = NCombatRoom.Instance;
		ownerNode = ((instance3 != null) ? instance3.GetCreatureNode(model.Owner) : null);
		if (ownerNode == null || !GodotObject.IsInstanceValid((GodotObject)(object)ownerNode))
		{
			ValencinaProbeLog.Info("hover-skip-missing-owner-node", "Skipped power hover for " + ((AbstractModel)model).Id.Entry + ": owner node unavailable.");
			return false;
		}
		try
		{
			tips = model.HoverTips?.ToList() ?? new List<IHoverTip>();
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[NPowerHoverGuard] Suppressed invalid power hover tips for " + ((AbstractModel)model).Id.Entry + ": " + ex.Message, 1);
			return false;
		}
		object? obj3 = IconField?.GetValue(instance);
		icon = (TextureRect?)((obj3 is TextureRect) ? obj3 : null);
		return true;
	}
}
