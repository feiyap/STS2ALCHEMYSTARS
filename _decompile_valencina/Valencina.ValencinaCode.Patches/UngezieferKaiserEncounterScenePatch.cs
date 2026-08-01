using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(EncounterModel), "CreateScene")]
internal static class UngezieferKaiserEncounterScenePatch
{
	private static bool Prefix(EncounterModel __instance, ref Control __result)
	{
		if (UngezieferKaiserFinalBossController.IsKaiserEncounterId(__instance) || !UngezieferKaiserEncounterReplacementPatch.ShouldUseKaiserAssets(__instance))
		{
			return true;
		}
		try
		{
			PackedScene scene = PreloadManager.Cache.GetScene("res://Valencina/scenes/encounters/ungeziefer_kaiser_background.tscn");
			__result = scene.Instantiate<Control>((GenEditState)0);
			MainFile.Logger.Info("[UngezieferKaiser] Applied custom encounter slot scene.", 1);
			return false;
		}
		catch (Exception value)
		{
			MainFile.Logger.Warn($"[UngezieferKaiser] Failed to load custom encounter slot scene: {value}", 1);
			return true;
		}
	}
}
