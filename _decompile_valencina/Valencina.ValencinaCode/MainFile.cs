using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using STS2RitsuLib;
using STS2RitsuLib.Interop;
using Valencina.ValencinaCode.Content;
using Valencina.ValencinaCode.Monsters;
using Valencina.ValencinaCode.Patches;
using Valencina.ValencinaCode.Powers;
using Valencina.ValencinaCode.Settings;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode;

[ModInitializer("Initialize")]
[ScriptPath("res://ValencinaCode/MainFile.cs")]
public class MainFile : Node
{
	public class MethodName : MethodName
	{
		public static readonly StringName DiagnosticInfo = StringName.op_Implicit("DiagnosticInfo");

		public static readonly StringName Initialize = StringName.op_Implicit("Initialize");

		public static readonly StringName RegisterValencinaSceneConversions = StringName.op_Implicit("RegisterValencinaSceneConversions");

		public static readonly StringName ValidateImportantResources = StringName.op_Implicit("ValidateImportantResources");
	}

	public class PropertyName : PropertyName
	{
	}

	public class SignalName : SignalName
	{
	}

	public const string ModId = "Valencina";

	public const string ResPath = "res://Valencina";

	public const string CharacterVisualSourceScene = "res://Valencina/scenes/characters/creature_visuals_valencina.tscn";

	public const string CharacterSelectBgSourceScene = "res://Valencina/scenes/characters/char_select_bg_valencina.tscn";

	public const string RestSiteSourceScene = "res://Valencina/scenes/characters/rest_site_valencina.tscn";

	public const string MerchantSourceScene = "res://Valencina/scenes/characters/merchant_valencina.tscn";

	public const string AmmoUiSourceScene = "res://Valencina/scenes/ui/ammo_counter_ui.tscn";

	public const string CharacterVisualExportedScene = "res://.godot/exported/133200997/export-c774c375ea76b5d74364fd78f7485b90-creature_visuals_valencina.scn";

	public const string CharacterSelectBgExportedScene = "res://.godot/exported/133200997/export-213a1274616a55da5e8da78934684707-char_select_bg_valencina.scn";

	public const string RestSiteExportedScene = "res://.godot/exported/133200997/export-8844481cc9f973f98c7194a302d2781f-rest_site_valencina.scn";

	public const string MerchantExportedScene = "res://.godot/exported/133200997/export-07d6ab4a2d5ada55c9d138bff4121c22-merchant_valencina.scn";

	public const string AmmoUiExportedScene = "res://.godot/exported/133200997/export-1d4078a981c6d413366965d8f8c90f2e-ammo_counter_ui.scn";

	public static readonly string[] CharacterVisualSceneCandidates = new string[2] { "res://Valencina/scenes/characters/creature_visuals_valencina.tscn", "res://.godot/exported/133200997/export-c774c375ea76b5d74364fd78f7485b90-creature_visuals_valencina.scn" };

	public static readonly string[] CharacterSelectBgSceneCandidates = new string[2] { "res://Valencina/scenes/characters/char_select_bg_valencina.tscn", "res://.godot/exported/133200997/export-213a1274616a55da5e8da78934684707-char_select_bg_valencina.scn" };

	public static readonly string[] RestSiteSceneCandidates = new string[2] { "res://Valencina/scenes/characters/rest_site_valencina.tscn", "res://.godot/exported/133200997/export-8844481cc9f973f98c7194a302d2781f-rest_site_valencina.scn" };

	public static readonly string[] MerchantSceneCandidates = new string[2] { "res://Valencina/scenes/characters/merchant_valencina.tscn", "res://.godot/exported/133200997/export-07d6ab4a2d5ada55c9d138bff4121c22-merchant_valencina.scn" };

	public static readonly string[] AmmoUiSceneCandidates = new string[2] { "res://.godot/exported/133200997/export-1d4078a981c6d413366965d8f8c90f2e-ammo_counter_ui.scn", "res://Valencina/scenes/ui/ammo_counter_ui.tscn" };

	public static readonly string[] AllSceneCandidates = new string[5] { "res://Valencina/scenes/characters/creature_visuals_valencina.tscn", "res://Valencina/scenes/characters/char_select_bg_valencina.tscn", "res://Valencina/scenes/characters/rest_site_valencina.tscn", "res://Valencina/scenes/characters/merchant_valencina.tscn", "res://Valencina/scenes/ui/ammo_counter_ui.tscn" };

	public const string CharacterIconTexture = "res://Valencina/images/charui/character_icon_valencina.png";

	public const string CharacterIconScene = "res://Valencina/scenes/ui/character_icons/valencina_icon.tscn";

	public const string CharacterSelectIcon = "res://Valencina/images/charui/char_select_valencina.png";

	public const string CharacterSelectLockedIcon = "res://Valencina/images/charui/char_select_valencina_locked.png";

	public const string MapMarkerIcon = "res://Valencina/images/charui/map_marker_valencina.png";

	public const string CharacterPortrait = "res://Valencina/images/charui/portrait_valencina.png";

	public const string BigEnergyIcon = "res://Valencina/images/charui/big_energy.png";

	public const string TextEnergyIcon = "res://Valencina/images/charui/text_energy.png";

	public const string RestSiteBody = "res://Valencina/images/charui/rest_site_valencina_body.png";

	public const string RestSiteGlow = "res://Valencina/images/charui/rest_site_valencina_fire_glow.png";

	public const string AmmoCylinder = "res://Valencina/images/ui/ammo/ammo_cylinder_ui.png";

	public const string InstantEnchantmentIcon = "res://Valencina/images/enchantments/instant_enchantment.png";

	public const string ImperfectForesightEyeRelicIcon = "res://Valencina/images/relics/imperfect_foresight_eye.png";

	public const string ImperfectForesightEyeBigRelicIcon = "res://Valencina/images/relics/big/imperfect_foresight_eye.png";

	public const string BernoullitMemoryRelicIcon = "res://Valencina/images/relics/bernoullit_memory.png";

	public const string BernoullitMemoryBigRelicIcon = "res://Valencina/images/relics/big/bernoullit_memory.png";

	public const string ShinEffectShader = "res://Valencina/shaders/vfx/shin_effect.gdshader";

	public const string ShinMainTexture = "res://Valencina/images/vfx/shin/shin.png";

	public const string ShinNoise03Texture = "res://Valencina/images/vfx/shin/noise_03.png";

	public const string ShinNoise04Texture = "res://Valencina/images/vfx/shin/noise_04.png";

	public const string ShinThreadNoiseTexture = "res://Valencina/images/vfx/shin/thread_noise.png";

	public const string ValencinaShinPowerIcon = "res://Valencina/images/powers/valencina_shin_power.png";

	public const string ValencinaShinPowerBigIcon = "res://Valencina/images/powers/big/valencina_shin_power.png";

	public const string PrecognitionOverheatIcon = "res://Valencina/images/powers/instant_foresight_power_overheat.png";

	public const string PrecognitionOverheatBigIcon = "res://Valencina/images/powers/big/instant_foresight_power_overheat.png";

	public static string CharacterVisualScene => ResolveExistingPath(CharacterVisualSceneCandidates);

	public static string CharacterSelectBgScene => ResolveExistingPath(CharacterSelectBgSceneCandidates);

	public static string RestSiteScene => ResolveExistingPath(RestSiteSceneCandidates);

	public static string MerchantScene => ResolveExistingPath(MerchantSceneCandidates);

	public static string AmmoUiScene => ResolveExistingPath(AmmoUiSceneCandidates);

	public static Logger Logger { get; } = new Logger("Valencina", (LogType)0);

	public static bool DiagnosticLogsEnabled => string.Equals(Environment.GetEnvironmentVariable("VALENCINA_DIAGNOSTIC_LOGS"), "1", StringComparison.OrdinalIgnoreCase);

	public static void DiagnosticInfo(string message)
	{
		if (DiagnosticLogsEnabled)
		{
			Logger.Info(message, 1);
		}
	}

	public static void Initialize()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		ModTypeDiscoveryHub.RegisterModAssembly("Valencina", executingAssembly);
		RitsuLibFramework.EnsureGodotScriptsRegistered(executingAssembly, Logger);
		ValencinaRitsuContent.Register();
		ValencinaModConfig.Register();
		RegisterValencinaSceneConversions();
		if (DiagnosticLogsEnabled)
		{
			ValidateImportantResources();
		}
		new Harmony("Valencina").PatchAll();
		DiagnosticInfo("[Precognition] runtime patches loaded: direct-dodge-v3 cumulative-threshold pre-block-defense-v2. Intent preview overlay disabled.");
	}

	private static void RegisterValencinaSceneConversions()
	{
		RegisterSceneConversions<NMerchantCharacter>((IEnumerable<string>)MerchantSceneCandidates);
		RegisterSceneConversions<NRestSiteCharacter>((IEnumerable<string>)RestSiteSceneCandidates);
	}

	private static void ValidateImportantResources()
	{
		List<string> list = AllSceneCandidates.ToList();
		list.AddRange(new string[61]
		{
			"res://Valencina/images/charui/character_icon_valencina.png", "res://Valencina/scenes/ui/character_icons/valencina_icon.tscn", "res://Valencina/images/charui/char_select_valencina.png", "res://Valencina/images/charui/char_select_valencina_locked.png", "res://Valencina/images/charui/map_marker_valencina.png", "res://Valencina/images/charui/portrait_valencina.png", "res://Valencina/images/charui/big_energy.png", "res://Valencina/images/charui/text_energy.png", "res://Valencina/images/charui/rest_site_valencina_body.png", "res://Valencina/images/charui/rest_site_valencina_fire_glow.png",
			"res://Valencina/images/ui/ammo/ammo_cylinder_ui.png", "res://Valencina/images/enchantments/instant_enchantment.png", "res://Valencina/images/relics/imperfect_foresight_eye.png", "res://Valencina/images/relics/big/imperfect_foresight_eye.png", "res://Valencina/images/relics/bernoullit_memory.png", "res://Valencina/images/relics/big/bernoullit_memory.png", "res://scenes/events/background_scenes/thumb_advisor.tscn", "res://Valencina/images/ui/run_history/thumb_advisor.png", "res://Valencina/images/ui/run_history/thumb_advisor_outline.png", "res://scenes/events/background_scenes/limbus_company_headquarters.tscn",
			"res://Valencina/images/ui/run_history/limbus_company_headquarters.png", "res://Valencina/images/ui/run_history/limbus_company_headquarters_outline.png", "res://scenes/events/background_scenes/rien.tscn", "res://Valencina/images/ui/run_history/rien.png", "res://Valencina/images/ui/run_history/rien_outline.png", "res://scenes/events/background_scenes/stars.tscn", "res://Valencina/images/events/stars_background.webp", "res://Valencina/images/ui/map/stars_map_icon.webp", "res://Valencina/images/events/vagrant.png", "res://scenes/events/background_scenes/cockroach_emperor_phase_choice.tscn",
			"res://Valencina/images/events/cockroach_emperor_phase_choice_background.png", "res://scenes/events/background_scenes/lucio_choice.tscn", "res://Valencina/images/events/lucio_choice_background.png", "res://Valencina/shaders/vfx/shin_effect.gdshader", "res://Valencina/images/vfx/shin/shin.png", "res://Valencina/images/vfx/shin/noise_03.png", "res://Valencina/images/vfx/shin/noise_04.png", "res://Valencina/images/vfx/shin/thread_noise.png", "res://Valencina/images/powers/valencina_shin_power.png", "res://Valencina/images/powers/big/valencina_shin_power.png",
			"res://Valencina/images/powers/instant_foresight_power_overheat.png", "res://Valencina/images/powers/big/instant_foresight_power_overheat.png", "res://Valencina/audio/attack/atk1_1.mp3", "res://Valencina/audio/attack/atk1_2.mp3", "res://Valencina/audio/attack/atk1_3.mp3", "res://Valencina/audio/attack/atk2_1.mp3", "res://Valencina/audio/attack/atk2_2.mp3", "res://Valencina/audio/attack/atk2_3.mp3", "res://Valencina/audio/disposal/voice.ogg", "res://Valencina/audio/disposal/voice_2.mp3",
			"res://Valencina/audio/disposal/dis_1.ogg", "res://Valencina/audio/disposal/dis_2.ogg", "res://Valencina/audio/disposal/dis_3.ogg", "res://Valencina/audio/disposal/dis_4.ogg", "res://Valencina/audio/disposal/dis_5.ogg", "res://Valencina/audio/ui/char_select.mp3", "res://Valencina/audio/ui/cylinder_tick.mp3", "res://Valencina/audio/reload/reload_once.mp3", "res://Valencina/audio/voice/precognition/overheat.mp3", "res://Valencina/audio/death/death.mp3",
			"res://Valencina/audio/music/boss_cp9_1_2.mp3"
		}.Concat(PowerIconRegistry.AllExplicitIconPaths).Concat(ValencinaVoiceSfx.AssetPaths).Concat(ValencinaAnimation.Attack2AssetPaths)
			.Concat(ValencinaAnimation.DisposalAssetPaths)
			.Concat(UngezieferKaiserAssets.AllAssetPaths)
			.Concat(Act4EliteAssets.AllAssetPaths));
		int num = 0;
		foreach (string item in list)
		{
			if (!ResourceLoader.Exists(item, ""))
			{
				num++;
				Logger.Warn("[ValencinaResources] MISSING: " + item, 1);
			}
		}
		if (num == 0)
		{
			DiagnosticInfo($"[ValencinaResources] Checked {list.Count} resources; none missing.");
		}
		else
		{
			Logger.Warn($"[ValencinaResources] Checked {list.Count} resources; missing={num}.", 1);
		}
	}

	public static string ResolveExistingPath(IReadOnlyList<string> candidates)
	{
		foreach (string candidate in candidates)
		{
			if (ResourceLoader.Exists(candidate, ""))
			{
				return candidate;
			}
		}
		if (candidates.Count <= 0)
		{
			return string.Empty;
		}
		return candidates[0];
	}

	private static void RegisterSceneConversions<TNode>(IEnumerable<string> paths) where TNode : Node
	{
		foreach (string path in paths)
		{
			try
			{
				DiagnosticInfo("[ValencinaResources] Scene conversion retained by Godot/RitsuLib asset path: " + typeof(TNode).Name + " <- " + path);
			}
			catch (Exception ex)
			{
				Logger.Warn("[ValencinaResources] Failed scene conversion registration for " + path + ": " + ex.Message, 1);
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		return new List<MethodInfo>(4)
		{
			new MethodInfo(MethodName.DiagnosticInfo, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)33, new List<PropertyInfo>
			{
				new PropertyInfo((Type)4, StringName.op_Implicit("message"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.Initialize, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)33, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.RegisterValencinaSceneConversions, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)33, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.ValidateImportantResources, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)33, (List<PropertyInfo>)null, (List<Variant>)null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		if ((ref method) == MethodName.DiagnosticInfo && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			DiagnosticInfo(VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.Initialize && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			Initialize();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.RegisterValencinaSceneConversions && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			RegisterValencinaSceneConversions();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.ValidateImportantResources && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			ValidateImportantResources();
			ret = default(godot_variant);
			return true;
		}
		return ((Node)this).InvokeGodotClassMethod(ref method, args, ref ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		if ((ref method) == MethodName.DiagnosticInfo && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			DiagnosticInfo(VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.Initialize && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			Initialize();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.RegisterValencinaSceneConversions && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			RegisterValencinaSceneConversions();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.ValidateImportantResources && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			ValidateImportantResources();
			ret = default(godot_variant);
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if ((ref method) == MethodName.DiagnosticInfo)
		{
			return true;
		}
		if ((ref method) == MethodName.Initialize)
		{
			return true;
		}
		if ((ref method) == MethodName.RegisterValencinaSceneConversions)
		{
			return true;
		}
		if ((ref method) == MethodName.ValidateImportantResources)
		{
			return true;
		}
		return ((Node)this).HasGodotClassMethod(ref method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		((GodotObject)this).SaveGodotObjectData(info);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		((GodotObject)this).RestoreGodotObjectData(info);
	}
}
