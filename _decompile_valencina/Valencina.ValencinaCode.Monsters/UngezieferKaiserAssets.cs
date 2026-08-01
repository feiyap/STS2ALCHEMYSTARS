using System.Collections.Generic;
using System.Linq;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Monsters;

public static class UngezieferKaiserAssets
{
	public const string VisualScene = "res://Valencina/scenes/monsters/ungeziefer_kaiser.tscn";

	public const string SlotScene = "res://Valencina/scenes/encounters/ungeziefer_kaiser_background.tscn";

	public const string BackgroundScene = "res://scenes/backgrounds/valencina-ungeziefer_kaiser_encounter/valencina-ungeziefer_kaiser_encounter_background.tscn";

	public const string BackgroundLayer = "res://scenes/backgrounds/valencina-ungeziefer_kaiser_encounter/layers/valencina-ungeziefer_kaiser_encounter_bg_00_a.tscn";

	public const string BackgroundTexture = "res://Valencina/images/monsters/ungeziefer_kaiser/background.png";

	public const string BackgroundTitle = "valencina-ungeziefer_kaiser_encounter";

	public const string MusicRelative = "music/ungeziefer_kaiser.mp3";

	public const string MusicPath = "res://Valencina/audio/music/ungeziefer_kaiser.mp3";

	public const string AttackVoice1Relative = "monsters/ungeziefer_kaiser/attack_1.wav";

	public const string AttackVoice2Relative = "monsters/ungeziefer_kaiser/attack_2.wav";

	public const string AttackVoice3Relative = "monsters/ungeziefer_kaiser/attack_3.wav";

	public const string TurnVoice1Relative = "monsters/ungeziefer_kaiser/turn_1.wav";

	public const string TurnVoice2Relative = "monsters/ungeziefer_kaiser/turn_2.wav";

	public static readonly IReadOnlyList<string> AttackVoiceRelativePaths = new _003C_003Ez__ReadOnlyArray<string>(new string[3] { "monsters/ungeziefer_kaiser/attack_1.wav", "monsters/ungeziefer_kaiser/attack_2.wav", "monsters/ungeziefer_kaiser/attack_3.wav" });

	public static readonly IReadOnlyList<string> TurnVoiceRelativePaths = new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "monsters/ungeziefer_kaiser/turn_1.wav", "monsters/ungeziefer_kaiser/turn_2.wav" });

	public const string MapBossIconBasePath = "res://Valencina/images/ui/run_history/ungeziefer_kaiser_encounter";

	public const string RunHistoryIcon = "res://Valencina/images/ui/run_history/ungeziefer_kaiser_encounter.png";

	public const string RunHistoryIconOutline = "res://Valencina/images/ui/run_history/ungeziefer_kaiser_encounter_outline.png";

	private static readonly string[] BackgroundAssetPaths = new string[6] { "res://scenes/backgrounds/valencina-ungeziefer_kaiser_encounter/valencina-ungeziefer_kaiser_encounter_background.tscn", "res://scenes/backgrounds/valencina-ungeziefer_kaiser_encounter/layers/valencina-ungeziefer_kaiser_encounter_bg_00_a.tscn", "res://scenes/backgrounds/valencinasts2-ungeziefer_kaiser_encounter/valencinasts2-ungeziefer_kaiser_encounter_background.tscn", "res://scenes/backgrounds/valencinasts2-ungeziefer_kaiser_encounter/layers/valencinasts2-ungeziefer_kaiser_encounter_bg_00_a.tscn", "res://scenes/backgrounds/ungeziefer_kaiser_encounter/ungeziefer_kaiser_encounter_background.tscn", "res://scenes/backgrounds/ungeziefer_kaiser_encounter/layers/ungeziefer_kaiser_encounter_bg_00_a.tscn" };

	private static readonly string[] VfxAssetPaths = new string[3] { "res://scenes/vfx/ui/vfx_buff_applied.tscn", "res://scenes/vfx/ui/vfx_debuff_applied.tscn", "res://scenes/cards/overlays/infection.tscn" };

	public static IEnumerable<string> AllAssetPaths => new string[11]
	{
		"res://Valencina/scenes/monsters/ungeziefer_kaiser.tscn", "res://Valencina/scenes/encounters/ungeziefer_kaiser_background.tscn", "res://Valencina/images/monsters/ungeziefer_kaiser/background.png", "res://Valencina/audio/music/ungeziefer_kaiser.mp3", "res://Valencina/audio/monsters/ungeziefer_kaiser/attack_1.wav", "res://Valencina/audio/monsters/ungeziefer_kaiser/attack_2.wav", "res://Valencina/audio/monsters/ungeziefer_kaiser/attack_3.wav", "res://Valencina/audio/monsters/ungeziefer_kaiser/turn_1.wav", "res://Valencina/audio/monsters/ungeziefer_kaiser/turn_2.wav", "res://Valencina/images/ui/run_history/ungeziefer_kaiser_encounter.png",
		"res://Valencina/images/ui/run_history/ungeziefer_kaiser_encounter_outline.png"
	}.Concat(BackgroundAssetPaths).Concat(IdleFramePaths).Concat(AttackFramePaths)
		.Concat(VfxAssetPaths)
		.Concat(PowerIconRegistry.AllExplicitIconPaths)
		.Distinct();

	public static IEnumerable<string> IdleFramePaths
	{
		get
		{
			for (int i = 0; i < 60; i++)
			{
				yield return $"res://Valencina/images/monsters/ungeziefer_kaiser/idle/idle_{i:D3}.png";
			}
		}
	}

	public static IEnumerable<string> AttackFramePaths
	{
		get
		{
			string root = "res://Valencina/images/monsters/ungeziefer_kaiser/attack/";
			for (int i = 2; i <= 11; i++)
			{
				yield return $"{root}skill1_{i}.png";
			}
			for (int i = 2; i <= 9; i++)
			{
				yield return $"{root}skill2_{i}.png";
			}
			for (int i = 2; i <= 6; i++)
			{
				yield return $"{root}skill4_{i}.png";
			}
			yield return root + "skill4_7~9.png";
		}
	}
}
