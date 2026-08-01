using System;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
internal static class PrecognitionPowerIconRefreshPatch
{
	private static readonly FieldInfo? ModelField = AccessTools.Field(typeof(NPower), "_model");

	private static readonly FieldInfo? IconField = AccessTools.Field(typeof(NPower), "_icon");

	private static readonly FieldInfo? PowerFlashField = AccessTools.Field(typeof(NPower), "_powerFlash");

	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(typeof(NPower), "RefreshAmount", (Type[])null, (Type[])null) ?? throw new MissingMethodException(typeof(NPower).FullName, "RefreshAmount");
	}

	private static void Postfix(NPower __instance)
	{
		try
		{
			if (ModelField?.GetValue(__instance) is InstantForesightPower instantForesightPower)
			{
				object? obj = IconField?.GetValue(__instance);
				TextureRect val = (TextureRect)((obj is TextureRect) ? obj : null);
				if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
				{
					val.Texture = ((PowerModel)instantForesightPower).Icon;
				}
				object? obj2 = PowerFlashField?.GetValue(__instance);
				CpuParticles2D val2 = (CpuParticles2D)((obj2 is CpuParticles2D) ? obj2 : null);
				if (val2 != null && GodotObject.IsInstanceValid((GodotObject)(object)val2))
				{
					val2.Texture = ((PowerModel)instantForesightPower).BigIcon;
				}
			}
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[Precognition] Failed to refresh power icon: " + ex.Message, 1);
		}
	}
}
