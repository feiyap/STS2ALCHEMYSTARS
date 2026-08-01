using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Character;
using Valencina.ValencinaCode.Monsters;
using Valencina.ValencinaCode.Patches;
using Valencina.ValencinaCode.Settings;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Audio;

internal static class ValencinaMusicManager
{
	private const string DebugAudioPrefix = "res://debug_audio/";

	private static bool _overrideActive;

	private static int _currentSoundId = -1;

	private static string? _activeMusicPath;

	private static int _pendingTransientAudioStopRequest;

	private static bool _shutdownCleanupHandled;

	private static int _shutdownQuitCalls;

	private static FieldInfo? _proxyField;

	internal static bool IsOverrideActive => _overrideActive;

	internal static void StartBossMusicIfNeeded(CombatState? combatState)
	{
		ApplyDesiredTrack(DecideDesiredTrack(combatState, combatEnding: false));
	}

	internal static void OnActChanged()
	{
		ApplyDesiredTrack(DecideDesiredTrack(null, combatEnding: false));
	}

	internal static void ApplySettingsChange(bool fromSettingsUi = false)
	{
		ApplyDesiredTrack(DecideDesiredTrack(null, combatEnding: false));
	}

	internal static void ApplyMusicVolumeSettingChange()
	{
		string text = DecideDesiredTrack(null, combatEnding: false);
		if (text != null)
		{
			_activeMusicPath = null;
			ApplyDesiredTrack(text);
		}
		else if (_overrideActive)
		{
			DeactivateOverride(resumeVanilla: true);
		}
	}

	internal static void StopBossMusicAfterCombat(bool stopTransientAudioImmediately = true, bool restoreVanillaMusic = true)
	{
		if (stopTransientAudioImmediately)
		{
			StopAllTransientAudio("combat-end");
		}
		else
		{
			StopAllTransientAudioAfterDelay("combat-end", 1800);
		}
		ApplyDesiredTrack(DecideDesiredTrack(null, combatEnding: true));
	}

	private static string? DecideDesiredTrack(CombatState? explicitCombatState, bool combatEnding)
	{
		if (!combatEnding)
		{
			object obj = explicitCombatState;
			if (obj == null)
			{
				CombatManager instance = CombatManager.Instance;
				if (instance == null || !instance.IsInProgress)
				{
					obj = null;
				}
				else
				{
					CombatManager instance2 = CombatManager.Instance;
					obj = ((instance2 != null) ? instance2.DebugOnlyGetState() : null);
				}
			}
			CombatState val = (CombatState)obj;
			if (val != null && TryGetReplacementMusic(val, out string musicPath))
			{
				return musicPath;
			}
		}
		if (ValencinaModConfig.EnableBossMusicReplacement && IsInValencinaAct4())
		{
			return "res://Valencina/audio/music/act4_run.mp3";
		}
		return null;
	}

	private static void ApplyDesiredTrack(string? desiredMusicPath)
	{
		if (desiredMusicPath == null)
		{
			if (_overrideActive)
			{
				DeactivateOverride(resumeVanilla: true);
			}
		}
		else if (!_overrideActive || !(_activeMusicPath == desiredMusicPath) || _currentSoundId < 0)
		{
			StartReplacementMusic(desiredMusicPath);
		}
	}

	private static bool IsInValencinaAct4()
	{
		try
		{
			RunManager instance = RunManager.Instance;
			if (instance == null)
			{
				return false;
			}
			if (!UngezieferKaiserFinalBossController.TryGetRunState(instance, out IRunState runState) || runState == null)
			{
				return false;
			}
			int currentActIndex = runState.CurrentActIndex;
			if (currentActIndex < 0 || currentActIndex >= runState.Acts.Count)
			{
				return false;
			}
			return UngezieferKaiserFinalBossController.IsValencinaAct4(runState.Acts[currentActIndex]);
		}
		catch
		{
			return false;
		}
	}

	internal static void StopAllModMusicForMainMenu()
	{
		StopAllTransientAudio("main-menu");
		DeactivateOverride(resumeVanilla: false);
	}

	internal static void StopAllModMusicImmediatelyForShutdown()
	{
		_shutdownQuitCalls++;
		if (_shutdownCleanupHandled)
		{
			ValencinaProbeLog.Warn("music-shutdown-repeat", $"NGame.Quit cleanup requested again; skipping repeated audio cleanup. quitCalls={_shutdownQuitCalls}.", 20);
		}
		else
		{
			_shutdownCleanupHandled = true;
			StopAllTransientAudio("shutdown");
			DeactivateOverride(resumeVanilla: false);
		}
	}

	private static bool TryGetReplacementMusic(CombatState? combatState, out string musicPath)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Invalid comparison between Unknown and I4
		musicPath = string.Empty;
		if (combatState == null || !ValencinaModConfig.EnableBossMusicReplacement)
		{
			return false;
		}
		if (CombatHasUngezieferKaiser(combatState))
		{
			musicPath = "res://Valencina/audio/music/ungeziefer_kaiser.mp3";
			return true;
		}
		EncounterModel encounter = combatState.Encounter;
		if (encounter != null && (int)encounter.RoomType == 3 && CombatHasValencina(combatState))
		{
			musicPath = "res://Valencina/audio/music/boss_cp9_1_2.mp3";
			return true;
		}
		EncounterModel encounter2 = combatState.Encounter;
		if (encounter2 != null && (int)encounter2.RoomType == 2 && CombatHasAct4Elite(combatState))
		{
			musicPath = "res://Valencina/audio/music/boss_cp9_1_2.mp3";
			return true;
		}
		return false;
	}

	private static bool CombatHasValencina(CombatState combatState)
	{
		try
		{
			return combatState.Players.Any((Player player) => ((player != null) ? player.Character : null) is Valencina.ValencinaCode.Character.Valencina);
		}
		catch
		{
			return false;
		}
	}

	private static bool CombatHasUngezieferKaiser(CombatState combatState)
	{
		try
		{
			return combatState.Enemies.Any((Creature enemy) => ((enemy != null) ? enemy.Monster : null) is UngezieferKaiser);
		}
		catch
		{
			return false;
		}
	}

	private static bool CombatHasAct4Elite(CombatState combatState)
	{
		try
		{
			return combatState.Enemies.Any(delegate(Creature enemy)
			{
				MonsterModel val = ((enemy != null) ? enemy.Monster : null);
				return (val is Act4EliteRodya || val is Act4EliteHeathcliff || val is Act4EliteGregor) ? true : false;
			});
		}
		catch
		{
			return false;
		}
	}

	private static void StartReplacementMusic(string musicPath)
	{
		try
		{
			StopOurSound();
			_overrideActive = true;
			SilenceVanillaMusic();
			if (!PlayThroughDebugAudio(musicPath, out var soundId))
			{
				DeactivateOverride(resumeVanilla: true);
				return;
			}
			_currentSoundId = soundId;
			_activeMusicPath = musicPath;
			MainFile.Logger.Info($"[ValencinaMusic] Replacement combat music started via NDebugAudioManager: {musicPath} (id={soundId})", 1);
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[ValencinaMusic] Failed to start replacement music: " + ex.GetType().Name + ": " + ex.Message, 1);
			DeactivateOverride(resumeVanilla: true);
		}
	}

	private static bool PlayThroughDebugAudio(string musicPath, out int soundId)
	{
		soundId = -1;
		NDebugAudioManager instance = NDebugAudioManager.Instance;
		if (instance == null)
		{
			MainFile.Logger.Warn("[ValencinaMusic] NDebugAudioManager.Instance missing; replacement music skipped: " + musicPath, 1);
			return false;
		}
		AudioStream val = GD.Load<AudioStream>(musicPath);
		if (val == null)
		{
			MainFile.Logger.Warn("[ValencinaMusic] Replacement music resource was not loaded: " + musicPath, 1);
			return false;
		}
		EnableLoop(val);
		string debugAudioStreamName = GetDebugAudioStreamName(musicPath);
		PreloadManager.Cache.SetAsset("res://debug_audio/" + debugAudioStreamName, (Resource)(object)val);
		soundId = instance.Play(debugAudioStreamName, GetMusicInstanceVolumeLinear(), (PitchVariance)0);
		return soundId >= 0;
	}

	private static string GetDebugAudioStreamName(string musicPath)
	{
		string fileName = Path.GetFileName(musicPath);
		if (!string.IsNullOrEmpty(fileName))
		{
			return fileName;
		}
		return "valencina_replacement.mp3";
	}

	private static void EnableLoop(AudioStream stream)
	{
		AudioStreamOggVorbis val = (AudioStreamOggVorbis)(object)((stream is AudioStreamOggVorbis) ? stream : null);
		if (val != null)
		{
			val.Loop = true;
			return;
		}
		AudioStreamMP3 val2 = (AudioStreamMP3)(object)((stream is AudioStreamMP3) ? stream : null);
		if (val2 != null)
		{
			val2.Loop = true;
		}
	}

	private static void DeactivateOverride(bool resumeVanilla)
	{
		StopOurSound();
		bool overrideActive = _overrideActive;
		_overrideActive = false;
		_activeMusicPath = null;
		if (overrideActive && resumeVanilla)
		{
			ResumeVanillaMusic();
		}
	}

	private static void StopOurSound()
	{
		if (_currentSoundId < 0)
		{
			return;
		}
		int currentSoundId = _currentSoundId;
		_currentSoundId = -1;
		try
		{
			NDebugAudioManager instance = NDebugAudioManager.Instance;
			if (instance != null)
			{
				instance.Stop(currentSoundId, 0.6f);
			}
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn($"[ValencinaMusic] Failed to stop replacement music (id={currentSoundId}): {ex.GetType().Name}: {ex.Message}", 1);
		}
	}

	private static void SilenceVanillaMusic()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			NRunMusicController instance = NRunMusicController.Instance;
			if (instance != null)
			{
				if ((object)_proxyField == null)
				{
					_proxyField = typeof(NRunMusicController).GetField("_proxy", BindingFlags.Instance | BindingFlags.NonPublic);
				}
				object? obj = _proxyField?.GetValue(instance);
				Node val = (Node)((obj is Node) ? obj : null);
				if (val != null)
				{
					((GodotObject)val).Call(StringName.op_Implicit("stop_music"), Array.Empty<Variant>());
				}
				else
				{
					MainFile.Logger.Warn("[ValencinaMusic] Could not access NRunMusicController proxy; leaving vanilla music as-is to avoid the rest-site ambience crash.", 1);
				}
			}
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[ValencinaMusic] Failed to stop vanilla music event: " + ex.GetType().Name + ": " + ex.Message, 1);
		}
	}

	private static void ResumeVanillaMusic()
	{
		try
		{
			NRunMusicController instance = NRunMusicController.Instance;
			if (instance != null)
			{
				instance.UpdateMusic();
			}
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[ValencinaMusic] Failed to resume vanilla music: " + ex.GetType().Name + ": " + ex.Message, 1);
		}
	}

	private static float GetMusicInstanceVolumeLinear()
	{
		return ValencinaModConfig.BossMusicVolume;
	}

	private static void StopAllTransientAudio(string reason)
	{
		try
		{
			ValencinaVoiceSfx.StopAll(reason);
			ValencinaLocalSfx.StopAllTransientSfx(reason);
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn($"[ValencinaMusic] Failed to stop transient audio during {reason}: {ex.GetType().Name}: {ex.Message}", 1);
		}
	}

	private static void StopAllTransientAudioAfterDelay(string reason, int delayMs)
	{
		int request = Interlocked.Increment(ref _pendingTransientAudioStopRequest);
		StopAllTransientAudioAfterDelayAsync(reason, delayMs, request);
	}

	private static async Task StopAllTransientAudioAfterDelayAsync(string reason, int delayMs, int request)
	{
		try
		{
			await Task.Delay(Math.Max(0, delayMs));
			if (request == Volatile.Read(in _pendingTransientAudioStopRequest))
			{
				StopAllTransientAudio(reason + "-grace");
			}
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn($"[ValencinaMusic] Failed to schedule delayed transient audio stop during {reason}: {ex.GetType().Name}: {ex.Message}", 1);
		}
	}
}
