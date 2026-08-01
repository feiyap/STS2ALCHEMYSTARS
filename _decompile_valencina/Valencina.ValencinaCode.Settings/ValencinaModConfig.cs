using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib;
using STS2RitsuLib.Settings;
using STS2RitsuLib.Utils.Persistence;
using STS2RitsuLib.Utils.Persistence.Migration;
using Valencina.ValencinaCode.Audio;
using Valencina.ValencinaCode.Patches;
using Valencina.ValencinaCode.UI;
using Valencina.ValencinaCode.Vfx;

namespace Valencina.ValencinaCode.Settings;

public sealed class ValencinaModConfig
{
	private const string SettingsKey = "settings";

	private const string SettingsFileName = "settings.json";

	private const double DefaultBossMusicVolume = 0.45;

	private static bool _registered;

	public bool EnableAmmoUiSetting { get; set; } = true;

	public bool EnableShinAuraEffectSetting { get; set; } = true;

	public bool EnableUngezieferKaiserBossSetting { get; set; } = true;

	public bool ForceUngezieferKaiserFinalBossSetting { get; set; }

	public bool EnableMultiplayerTeammateSfxSetting { get; set; }

	public bool EnableMultiplayerPingVoiceSetting { get; set; }

	public bool EnableBossMusicReplacementSetting { get; set; } = true;

	public double BossMusicVolumeSetting { get; set; } = 0.45;

	public bool EnableRienFollowUpAncientSetting { get; set; } = true;

	public bool EnableWarDifficultySetting { get; set; }

	public bool DisableAdvancedDifficultySelectionSetting { get; set; }

	public bool DisableAttackAnimationsSetting { get; set; }

	public bool DisableDisposalAnimationSetting { get; set; }

	public static bool EnableAmmoUi => Current.EnableAmmoUiSetting;

	public static bool DisableAmmoUi => !EnableAmmoUi;

	public static bool EnableShinAuraEffect => Current.EnableShinAuraEffectSetting;

	public static bool DisableShinAuraEffect => !EnableShinAuraEffect;

	public static bool EnableKaiserContent => Current.EnableUngezieferKaiserBossSetting;

	public static bool DisableKaiserContent => !EnableKaiserContent;

	public static bool EnableUngezieferKaiserBoss => EnableKaiserContent;

	public static bool ForceUngezieferKaiserFinalBoss => Current.ForceUngezieferKaiserFinalBossSetting;

	public static bool EnableMultiplayerTeammateSfx => Current.EnableMultiplayerTeammateSfxSetting;

	public static bool EnableMultiplayerPingVoice => Current.EnableMultiplayerPingVoiceSetting;

	public static bool EnableBossMusicReplacement => Current.EnableBossMusicReplacementSetting;

	public static float BossMusicVolume => (float)Math.Clamp(Current.BossMusicVolumeSetting, 0.0, 1.0);

	public static bool EnableRienFollowUpAncient => Current.EnableRienFollowUpAncientSetting;

	public static bool EnableWarDifficulty => Current.EnableWarDifficultySetting;

	public static bool DisableAdvancedDifficultySelection => Current.DisableAdvancedDifficultySelectionSetting;

	public static bool DisableAttackAnimations => Current.DisableAttackAnimationsSetting;

	public static bool DisableDisposalAnimation => Current.DisableDisposalAnimationSetting;

	private static ValencinaModConfig Current
	{
		get
		{
			try
			{
				return RitsuLibFramework.GetDataStore("Valencina").Get<ValencinaModConfig>("settings");
			}
			catch
			{
				return new ValencinaModConfig();
			}
		}
	}

	public static void Register()
	{
		if (_registered)
		{
			return;
		}
		try
		{
			_registered = true;
			RegisterStore();
			RegisterSettingsPage();
			ApplyStartupSettings();
			MainFile.DiagnosticInfo("[ValencinaConfig] RitsuLib settings page registered.");
		}
		catch (Exception value)
		{
			MainFile.Logger.Error($"[ValencinaConfig] Failed to initialize config defaults: {value}", 1);
		}
	}

	private static void RegisterStore()
	{
		using (RitsuLibFramework.BeginModDataRegistration("Valencina", true))
		{
			RitsuLibFramework.GetDataStore("Valencina").Register<ValencinaModConfig>("settings", "settings.json", (SaveScope)0, (Func<ValencinaModConfig>)(() => new ValencinaModConfig()), true, (ModDataMigrationConfig)null, (IEnumerable<IMigration>)null);
		}
	}

	private static void RegisterSettingsPage()
	{
		RitsuLibFramework.RegisterModSettings("Valencina", (Action<ModSettingsPageBuilder>)delegate(ModSettingsPageBuilder page)
		{
			page.WithModDisplayName(T("VALENCINA.mod_title", "Valencina")).WithTitle(T("VALENCINA.settings.title", "Valencina Settings")).AddSection("gameplay", (Action<ModSettingsSectionBuilder>)delegate(ModSettingsSectionBuilder section)
			{
				section.WithTitle(T("VALENCINA.settings.gameplay.title", "Gameplay")).AddToggle("enable_ammo_ui", T("VALENCINA.settings.enable_ammo_ui.title", "UI"), (IModSettingsValueBinding<bool>)(object)Bind((ValencinaModConfig s) => s.EnableAmmoUiSetting, delegate(ValencinaModConfig s, bool value)
				{
					s.EnableAmmoUiSetting = value;
				}, ApplyAmmoUiSettingChange), T("VALENCINA.settings.enable_ammo_ui.desc", "Show Valencina's combat UI."), (Func<bool>)null).AddToggle("enable_shin_aura_effect", T("VALENCINA.settings.enable_shin_aura_effect.title", "Shin visual effect"), (IModSettingsValueBinding<bool>)(object)Bind((ValencinaModConfig s) => s.EnableShinAuraEffectSetting, delegate(ValencinaModConfig s, bool value)
				{
					s.EnableShinAuraEffectSetting = value;
				}, ApplyShinAuraSettingChange), T("VALENCINA.settings.enable_shin_aura_effect.desc", "Show Shin aura effects."), (Func<bool>)null)
					.AddToggle("enable_ungeziefer_kaiser_boss", T("VALENCINA.settings.enable_ungeziefer_kaiser_boss.title", "Ungeziefer Kaiser content"), (IModSettingsValueBinding<bool>)(object)Bind((ValencinaModConfig s) => s.EnableUngezieferKaiserBossSetting, delegate(ValencinaModConfig s, bool value)
					{
						s.EnableUngezieferKaiserBossSetting = value;
					}, ApplyKaiserSettingChange), T("VALENCINA.settings.enable_ungeziefer_kaiser_boss.desc", "Allow Ungeziefer Kaiser and the three summoning relics. Turn off to remove them from follow-up Ancient options and repair leaked boss slots."), (Func<bool>)null)
					.AddToggle("force_ungeziefer_kaiser_final_boss", T("VALENCINA.settings.force_ungeziefer_kaiser_final_boss.title", "强制启动蜚蠊帝皇"), (IModSettingsValueBinding<bool>)(object)Bind((ValencinaModConfig s) => s.ForceUngezieferKaiserFinalBossSetting, delegate(ValencinaModConfig s, bool value)
					{
						s.ForceUngezieferKaiserFinalBossSetting = value;
					}, ApplyForcedKaiserSettingChange), T("VALENCINA.settings.force_ungeziefer_kaiser_final_boss.desc", "开启后无视蛆、蛾、蝇三件召唤遗物条件，直接启用第四层路线。"), (Func<bool>)null)
					.AddToggle("enable_rien_follow_up_ancient", T("VALENCINA.settings.enable_rien_follow_up_ancient.title", "Extra Ancient event"), (IModSettingsValueBinding<bool>)(object)Bind((ValencinaModConfig s) => s.EnableRienFollowUpAncientSetting, delegate(ValencinaModConfig s, bool value)
					{
						s.EnableRienFollowUpAncientSetting = value;
					}, ApplyRienAncientSettingChange), T("VALENCINA.settings.enable_rien_follow_up_ancient.desc", "Enable Valencina's additional follow-up Ancient choice page. Turn off to repair leaked follow-up Ancient rooms back to normal Ancient events."), (Func<bool>)null)
					.AddToggle("enable_war_difficulty", T("VALENCINA.settings.enable_war_difficulty.title", "War difficulty"), (IModSettingsValueBinding<bool>)(object)Bind((ValencinaModConfig s) => s.EnableWarDifficultySetting, delegate(ValencinaModConfig s, bool value)
					{
						s.EnableWarDifficultySetting = value;
					}), T("VALENCINA.settings.enable_war_difficulty.desc", "Start Valencina runs on War difficulty. For now, War uses all Ascension 10 rules."), (Func<bool>)null)
					.AddToggle("disable_advanced_difficulty_selection", T("VALENCINA.settings.disable_advanced_difficulty_selection.title", "Hide advanced difficulty"), (IModSettingsValueBinding<bool>)(object)Bind((ValencinaModConfig s) => s.DisableAdvancedDifficultySelectionSetting, delegate(ValencinaModConfig s, bool value)
					{
						s.DisableAdvancedDifficultySelectionSetting = value;
					}), T("VALENCINA.settings.disable_advanced_difficulty_selection.desc", "Hide War from the original difficulty selector. The War difficulty setting can still activate it."), (Func<bool>)null);
			})
				.AddSection("audio", (Action<ModSettingsSectionBuilder>)delegate(ModSettingsSectionBuilder section)
				{
					section.WithTitle(T("VALENCINA.settings.audio.title", "Audio")).AddToggle("enable_boss_music_replacement", T("VALENCINA.settings.enable_boss_music_replacement.title", "Replacement boss music"), (IModSettingsValueBinding<bool>)(object)Bind((ValencinaModConfig s) => s.EnableBossMusicReplacementSetting, delegate(ValencinaModConfig s, bool value)
					{
						s.EnableBossMusicReplacementSetting = value;
					}, ApplyBossMusicReplacementSettingChange), T("VALENCINA.settings.enable_boss_music_replacement.desc", "Replace boss battle music with Valencina tracks. Turn off to use vanilla boss music."), (Func<bool>)null).AddSlider("boss_music_volume", T("VALENCINA.settings.boss_music_volume.title", "Boss music volume"), (IModSettingsValueBinding<double>)(object)Bind((ValencinaModConfig s) => s.BossMusicVolumeSetting, delegate(ValencinaModConfig s, double value)
					{
						s.BossMusicVolumeSetting = value;
					}, ApplyBossMusicVolumeSettingChange), 0.0, 1.0, 0.05, (Func<double, string>)((double value) => $"{value * 100.0:0}%"), T("VALENCINA.settings.boss_music_volume.desc", "Adjust Valencina replacement boss music volume."))
						.AddToggle("enable_multiplayer_teammate_sfx", T("VALENCINA.settings.enable_multiplayer_teammate_sfx.title", "Teammate SFX"), (IModSettingsValueBinding<bool>)(object)Bind((ValencinaModConfig s) => s.EnableMultiplayerTeammateSfxSetting, delegate(ValencinaModConfig s, bool value)
						{
							s.EnableMultiplayerTeammateSfxSetting = value;
						}), T("VALENCINA.settings.enable_multiplayer_teammate_sfx.desc", "Allow teammates to hear reduced-volume Valencina character sound effects in multiplayer."), (Func<bool>)null)
						.AddToggle("enable_multiplayer_ping_voice", T("VALENCINA.settings.enable_multiplayer_ping_voice.title", "Ping voice"), (IModSettingsValueBinding<bool>)(object)Bind((ValencinaModConfig s) => s.EnableMultiplayerPingVoiceSetting, delegate(ValencinaModConfig s, bool value)
						{
							s.EnableMultiplayerPingVoiceSetting = value;
						}), T("VALENCINA.settings.enable_multiplayer_ping_voice.desc", "Play Valencina's ping voice on this client when you are Valencina and press the end-turn ping button."), (Func<bool>)null);
				})
				.AddSection("visual", (Action<ModSettingsSectionBuilder>)delegate(ModSettingsSectionBuilder section)
				{
					section.WithTitle(T("VALENCINA.settings.visual.title", "Visuals")).AddToggle("disable_attack_animations", T("VALENCINA.settings.disable_attack_animations.title", "Disable character attack animations"), (IModSettingsValueBinding<bool>)(object)Bind((ValencinaModConfig s) => s.DisableAttackAnimationsSetting, delegate(ValencinaModConfig s, bool value)
					{
						s.DisableAttackAnimationsSetting = value;
					}), T("VALENCINA.settings.disable_attack_animations.desc", "Turn off Valencina custom attack and block-hit animation playback."), (Func<bool>)null).AddToggle("disable_disposal_animation", T("VALENCINA.settings.disable_disposal_animation.title", "Disable Disposal animation"), (IModSettingsValueBinding<bool>)(object)Bind((ValencinaModConfig s) => s.DisableDisposalAnimationSetting, delegate(ValencinaModConfig s, bool value)
					{
						s.DisableDisposalAnimationSetting = value;
					}), T("VALENCINA.settings.disable_disposal_animation.desc", "Turn off Valencina's special Disposal cinematic. Disposal uses the shorter attack animation instead."), (Func<bool>)null);
				});
		}, (string)null);
	}

	private static ModSettingsText T(string key, string fallback)
	{
		return ModSettingsText.LocString("settings_ui", key, fallback);
	}

	private static ModSettingsValueBinding<ValencinaModConfig, bool> Bind(Func<ValencinaModConfig, bool> getter, Action<ValencinaModConfig, bool> setter, Action? afterApply = null)
	{
		return new ModSettingsValueBinding<ValencinaModConfig, bool>("Valencina", "settings", (SaveScope)0, getter, (Action<ValencinaModConfig, bool>)delegate(ValencinaModConfig settings, bool value)
		{
			setter(settings, value);
			ApplySettingChange(afterApply);
		});
	}

	private static ModSettingsValueBinding<ValencinaModConfig, double> Bind(Func<ValencinaModConfig, double> getter, Action<ValencinaModConfig, double> setter, Action? afterApply = null)
	{
		return new ModSettingsValueBinding<ValencinaModConfig, double>("Valencina", "settings", (SaveScope)0, getter, (Action<ValencinaModConfig, double>)delegate(ValencinaModConfig settings, double value)
		{
			setter(settings, Math.Clamp(value, 0.0, 1.0));
			ApplySettingChange(afterApply);
		});
	}

	private static void ApplyStartupSettings()
	{
		ApplySettingChange(delegate
		{
			ApplyAmmoUiSettingChange();
			ApplyShinAuraSettingChange();
			ApplyRienAncientSettingChange();
			ApplyKaiserSettingChange();
			ApplyForcedKaiserSettingChange();
			ApplyBossMusicReplacementSettingChange();
		});
	}

	private static void ApplySettingChange(Action? afterApply)
	{
		if (afterApply == null)
		{
			return;
		}
		try
		{
			afterApply();
		}
		catch (Exception value)
		{
			MainFile.Logger.Error($"[ValencinaConfig] Failed to apply live setting: {value}", 1);
		}
	}

	private static void ApplyAmmoUiSettingChange()
	{
		if (DisableAmmoUi)
		{
			AmmoUiSync.DestroyCombatUi();
		}
	}

	private static void ApplyShinAuraSettingChange()
	{
		if (DisableShinAuraEffect)
		{
			ShinAuraController.HideAllVisibleAuras();
		}
	}

	private static void ApplyBossMusicVolumeSettingChange()
	{
		ValencinaMusicManager.ApplyMusicVolumeSettingChange();
	}

	private static void ApplyBossMusicReplacementSettingChange()
	{
		ValencinaMusicManager.ApplySettingsChange(fromSettingsUi: true);
	}

	private static void ApplyRienAncientSettingChange()
	{
		if (!EnableRienFollowUpAncient)
		{
			if (IsCombatInProgress())
			{
				MainFile.Logger.Info("[ValencinaConfig] Deferred extra Ancient repair because combat is in progress.", 1);
			}
			else
			{
				ValencinaSpecialAncientPoolGuard.RepairCurrentRunAncients(log: true);
			}
		}
	}

	private static void ApplyKaiserSettingChange()
	{
		if (!EnableKaiserContent)
		{
			if (IsCombatInProgress())
			{
				MainFile.Logger.Info("[ValencinaConfig] Deferred Kaiser map repair because combat is in progress.", 1);
			}
			else
			{
				UngezieferKaiserFinalBossController.RepairCurrentRunAndRegenerateMapIfNeeded("settings disabled");
			}
		}
	}

	private static void ApplyForcedKaiserSettingChange()
	{
		if (IsCombatInProgress())
		{
			MainFile.Logger.Info("[ValencinaConfig] Deferred forced Kaiser final boss refresh because combat is in progress.", 1);
		}
		else if (EnableKaiserContent && ForceUngezieferKaiserFinalBoss)
		{
			if (RunManager.Instance != null && UngezieferKaiserFinalBossController.TryGetRunState(RunManager.Instance, out IRunState runState))
			{
				UngezieferKaiserFinalBossController.TryApplyAndRegenerateCurrentMap(runState);
			}
		}
		else
		{
			UngezieferKaiserFinalBossController.RepairCurrentRunAndRegenerateMapIfNeeded("forced Kaiser setting changed");
		}
	}

	private static bool IsCombatInProgress()
	{
		try
		{
			CombatManager instance = CombatManager.Instance;
			return instance != null && instance.IsInProgress;
		}
		catch
		{
			return false;
		}
	}
}
