using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NCombatBackground), "AddLayer")]
internal static class UngezieferKaiserBackgroundRemapPatch
{
	internal static string NormalizeSceneRemapPath(string path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return path;
		}
		if (!path.EndsWith(".tscn.remap", StringComparison.OrdinalIgnoreCase))
		{
			return path;
		}
		return path.Substring(0, path.Length - ".remap".Length);
	}

	private static void Prefix(ref string layerPath)
	{
		if (layerPath.Contains("ungeziefer_kaiser_encounter", StringComparison.OrdinalIgnoreCase) || layerPath.Contains("act4_elite_encounter", StringComparison.OrdinalIgnoreCase) || layerPath.Contains("war_ambush_encounter", StringComparison.OrdinalIgnoreCase))
		{
			string text = NormalizeSceneRemapPath(layerPath);
			if (!string.Equals(layerPath, text, StringComparison.Ordinal))
			{
				MainFile.Logger.Info("[UngezieferKaiser] Normalized background layer path " + layerPath + " -> " + text, 1);
				layerPath = text;
			}
		}
	}
}
