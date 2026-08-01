using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Characters.Visuals.Definition;
using STS2RitsuLib.Scaffolding.Visuals.Definition;
using Valencina.ValencinaCode.Monsters;
using Valencina.ValencinaCode.Patches;
using Valencina.ValencinaCode.Powers;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Character;

public class Valencina : ModCharacterTemplate<ValencinaCardPool, ValencinaRelicPool, ValencinaPotionPool>
{
	public const string CharacterId = "Valencina";

	public const string MultiplayerHandPointPath = "res://Valencina/images/ui/hands/multiplayer_hand_valencina_point.png";

	public const string MultiplayerHandRockPath = "res://Valencina/images/ui/hands/multiplayer_hand_valencina_rock.png";

	public const string MultiplayerHandPaperPath = "res://Valencina/images/ui/hands/multiplayer_hand_valencina_paper.png";

	public const string MultiplayerHandScissorsPath = "res://Valencina/images/ui/hands/multiplayer_hand_valencina_scissors.png";

	public static readonly Color Color = new Color("ffffff");

	public override Color NameColor => Color;

	public override CharacterGender Gender => (CharacterGender)0;

	public override int StartingHp => 70;

	public override int StartingGold => 99;

	public override float AttackAnimDelay => 0.3f;

	public override float CastAnimDelay => 0.3f;

	public override bool RequiresEpochAndTimeline => false;

	public override CharacterAssetProfile AssetProfile => new CharacterAssetProfile(new CharacterSceneAssetSet(MainFile.CharacterVisualScene, (string)null, MainFile.MerchantScene, MainFile.RestSiteScene), new CharacterUiAssetSet("res://Valencina/images/charui/character_icon_valencina.png", "res://Valencina/images/charui/character_icon_valencina.png", "res://Valencina/scenes/ui/character_icons/valencina_icon.tscn", MainFile.CharacterSelectBgScene, "res://Valencina/images/charui/char_select_valencina.png", "res://Valencina/images/charui/char_select_valencina_locked.png", (string)null, "res://Valencina/images/charui/map_marker_valencina.png"), (CharacterVfxAssetSet)null, (CharacterSpineAssetSet)null, new CharacterAudioAssetSet("event:/mods/valencina/ui/char_select", (string)null, (string)null, (string)null, (string)null), new CharacterMultiplayerAssetSet("res://Valencina/images/ui/hands/multiplayer_hand_valencina_point.png", "res://Valencina/images/ui/hands/multiplayer_hand_valencina_rock.png", "res://Valencina/images/ui/hands/multiplayer_hand_valencina_paper.png", "res://Valencina/images/ui/hands/multiplayer_hand_valencina_scissors.png"), (VisualCueSet)null, (CharacterWorldProceduralVisualSet)null, (CharacterVanillaRelicVisualOverride[])null, (CharacterVanillaPotionVisualOverride[])null, (CharacterVanillaCardVisualOverride[])null);

	protected override IEnumerable<string> ExtraAssetPaths
	{
		get
		{
			foreach (string item in _003C_003En__0())
			{
				yield return item;
			}
			yield return "res://Valencina/images/ui/hands/multiplayer_hand_valencina_point.png";
			yield return "res://Valencina/images/ui/hands/multiplayer_hand_valencina_rock.png";
			yield return "res://Valencina/images/ui/hands/multiplayer_hand_valencina_paper.png";
			yield return "res://Valencina/images/ui/hands/multiplayer_hand_valencina_scissors.png";
			yield return "res://Valencina/images/charui/character_icon_valencina.png";
			yield return "res://Valencina/scenes/ui/character_icons/valencina_icon.tscn";
			yield return "res://Valencina/images/charui/char_select_valencina.png";
			yield return "res://Valencina/images/charui/char_select_valencina_locked.png";
			yield return "res://Valencina/images/charui/map_marker_valencina.png";
			yield return "res://Valencina/images/charui/portrait_valencina.png";
			yield return "res://Valencina/images/charui/big_energy.png";
			yield return "res://Valencina/images/charui/text_energy.png";
			yield return MainFile.RestSiteScene;
			yield return MainFile.MerchantScene;
			yield return "res://Valencina/images/charui/rest_site_valencina_body.png";
			yield return "res://Valencina/images/charui/rest_site_valencina_fire_glow.png";
			yield return "res://Valencina/images/ui/ammo/ammo_cylinder_ui.png";
			yield return "res://Valencina/images/relics/imperfect_foresight_eye.png";
			yield return "res://Valencina/images/relics/big/imperfect_foresight_eye.png";
			yield return "res://Valencina/images/relics/bernoullit_memory.png";
			yield return "res://Valencina/images/relics/big/bernoullit_memory.png";
			yield return "res://Valencina/shaders/vfx/shin_effect.gdshader";
			yield return "res://Valencina/images/vfx/shin/shin.png";
			yield return "res://Valencina/images/vfx/shin/noise_03.png";
			yield return "res://Valencina/images/vfx/shin/noise_04.png";
			yield return "res://Valencina/images/vfx/shin/thread_noise.png";
			yield return "res://Valencina/images/powers/valencina_shin_power.png";
			yield return "res://Valencina/images/powers/big/valencina_shin_power.png";
			yield return "res://Valencina/audio/attack/atk1_1.mp3";
			yield return "res://Valencina/audio/attack/atk1_2.mp3";
			yield return "res://Valencina/audio/attack/atk1_3.mp3";
			yield return "res://Valencina/audio/attack/atk2_1.mp3";
			yield return "res://Valencina/audio/attack/atk2_2.mp3";
			yield return "res://Valencina/audio/attack/atk2_3.mp3";
			yield return "res://Valencina/audio/disposal/voice.ogg";
			yield return "res://Valencina/audio/disposal/voice_2.mp3";
			yield return "res://Valencina/audio/disposal/dis_1.ogg";
			yield return "res://Valencina/audio/disposal/dis_2.ogg";
			yield return "res://Valencina/audio/disposal/dis_3.ogg";
			yield return "res://Valencina/audio/disposal/dis_4.ogg";
			yield return "res://Valencina/audio/disposal/dis_5.ogg";
			yield return "res://Valencina/audio/ui/char_select.mp3";
			yield return "res://Valencina/audio/ui/cylinder_tick.mp3";
			yield return "res://Valencina/audio/reload/reload_once.mp3";
			yield return "res://Valencina/audio/voice/precognition/overheat.mp3";
			yield return "res://Valencina/audio/death/death.mp3";
			yield return "res://Valencina/audio/effects/tremor_burst.mp3";
			yield return "res://Valencina/audio/effects/tremor_stagger.mp3";
			yield return "res://Valencina/audio/music/boss_cp9_1_2.mp3";
			foreach (string allExplicitIconPath in PowerIconRegistry.AllExplicitIconPaths)
			{
				yield return allExplicitIconPath;
			}
			foreach (string allAssetPath in UngezieferKaiserAssets.AllAssetPaths)
			{
				yield return allAssetPath;
			}
			foreach (string allAssetPath2 in Act4EliteAssets.AllAssetPaths)
			{
				yield return allAssetPath2;
			}
			foreach (string assetPath in ValencinaVoiceSfx.AssetPaths)
			{
				yield return assetPath;
			}
			foreach (string attack2AssetPath in ValencinaAnimation.Attack2AssetPaths)
			{
				yield return attack2AssetPath;
			}
			foreach (string disposalAssetPath in ValencinaAnimation.DisposalAssetPaths)
			{
				yield return disposalAssetPath;
			}
		}
	}

	public override List<string> GetArchitectAttackVfx()
	{
		return new List<string>();
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private IEnumerable<string> _003C_003En__0()
	{
		return ((CharacterModel)this).ExtraAssetPaths;
	}
}
