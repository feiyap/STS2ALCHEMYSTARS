using System.Collections.Generic;
using System.Linq;

namespace Valencina.ValencinaCode.Monsters;

public static class GCompanyAmbushAssets
{
	public const string SoldierOneVisualScene = "res://Valencina/scenes/monsters/g_company_soldier_1.tscn";

	public const string SoldierTwoVisualScene = "res://Valencina/scenes/monsters/g_company_soldier_2.tscn";

	public const string SoldierThreeVisualScene = "res://Valencina/scenes/monsters/g_company_soldier_3.tscn";

	public const string MinisterVisualScene = "res://Valencina/scenes/monsters/g_company_minister.tscn";

	public static IEnumerable<string> SoldierOneAssetPaths => UnitAssetPaths("res://Valencina/scenes/monsters/g_company_soldier_1.tscn", "soldier_1", 17);

	public static IEnumerable<string> SoldierTwoAssetPaths => UnitAssetPaths("res://Valencina/scenes/monsters/g_company_soldier_2.tscn", "soldier_2", 17);

	public static IEnumerable<string> SoldierThreeAssetPaths => UnitAssetPaths("res://Valencina/scenes/monsters/g_company_soldier_3.tscn", "soldier_3", 17);

	public static IEnumerable<string> MinisterAssetPaths => UnitAssetPaths("res://Valencina/scenes/monsters/g_company_minister.tscn", "minister", 40);

	public static IEnumerable<string> AllAssetPaths => SoldierOneAssetPaths.Concat(SoldierTwoAssetPaths).Concat(SoldierThreeAssetPaths).Concat(MinisterAssetPaths)
		.Distinct();

	private static IEnumerable<string> UnitAssetPaths(string visualScene, string folder, int frameCount)
	{
		yield return visualScene;
		yield return "res://Valencina/scenes/monsters/sprite_frames/g_company_" + folder + "_idle.tres";
		for (int frame = 0; frame < frameCount; frame++)
		{
			yield return $"res://Valencina/images/monsters/g_company/{folder}/idle_{frame:D2}.png";
		}
	}
}
