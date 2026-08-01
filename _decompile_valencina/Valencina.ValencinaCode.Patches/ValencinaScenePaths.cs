using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Assets;

namespace Valencina.ValencinaCode.Patches;

internal static class ValencinaScenePaths
{
	public const string Portrait = "res://Valencina/images/charui/portrait_valencina.png";

	public const string CharacterIcon = "res://Valencina/images/charui/character_icon_valencina.png";

	public const string CharacterIconScene = "res://Valencina/scenes/ui/character_icons/valencina_icon.tscn";

	public const string CharacterSelectIcon = "res://Valencina/images/charui/char_select_valencina.png";

	public const string CharacterSelectLockedIcon = "res://Valencina/images/charui/char_select_valencina_locked.png";

	public const string MapMarker = "res://Valencina/images/charui/map_marker_valencina.png";

	public static string Visuals => MainFile.CharacterVisualScene;

	public static string RestSite => MainFile.RestSiteScene;

	public static string AmmoUi => MainFile.AmmoUiScene;

	public static PackedScene? LoadScene(IEnumerable<string> candidates)
	{
		foreach (string candidate in candidates)
		{
			try
			{
				PackedScene scene = PreloadManager.Cache.GetScene(candidate);
				if (scene != null)
				{
					return scene;
				}
			}
			catch (Exception ex)
			{
				MainFile.Logger.Warn("[ValencinaResources] Preload cache missed " + candidate + ": " + ex.Message, 1);
			}
			try
			{
				PackedScene val = ResourceLoader.Load<PackedScene>(candidate, (string)null, (CacheMode)1);
				if (val != null)
				{
					return val;
				}
			}
			catch (Exception ex2)
			{
				MainFile.Logger.Warn("[ValencinaResources] ResourceLoader failed " + candidate + ": " + ex2.Message, 1);
			}
		}
		return null;
	}
}
