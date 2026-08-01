using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using Valencina.ValencinaCode.Settings;

namespace Valencina.ValencinaCode.Utils;

public static class ValencinaLocalSfx
{
	private sealed class SfxSlot
	{
		public AudioStreamPlayer Player { get; }

		public string Path { get; }

		public ulong StartedAt { get; }

		public bool HighPriority { get; }

		public SfxSlot(AudioStreamPlayer player, string path, ulong startedAt, bool highPriority)
		{
			Player = player;
			Path = path;
			StartedAt = startedAt;
			HighPriority = highPriority;
		}
	}

	public const string CharacterSelectEvent = "event:/mods/valencina/ui/char_select";

	public const string AttackStartRelative = "attack/atk1_1.mp3";

	public const string AttackHitOneRelative = "attack/atk1_2.mp3";

	public const string AttackHitTwoRelative = "attack/atk1_3.mp3";

	public const string Attack2StartRelative = "attack/atk2_1.mp3";

	public const string Attack2HitOneRelative = "attack/atk2_2.mp3";

	public const string Attack2HitTwoRelative = "attack/atk2_3.mp3";

	public const string DisposalVoiceRelative = "disposal/voice.ogg";

	public const string DisposalVoiceAltRelative = "disposal/voice_2.mp3";

	public const string DisposalHitOneRelative = "disposal/dis_1.ogg";

	public const string DisposalHitTwoRelative = "disposal/dis_2.ogg";

	public const string DisposalHitThreeRelative = "disposal/dis_3.ogg";

	public const string DisposalHitFourRelative = "disposal/dis_4.ogg";

	public const string DisposalHitFiveRelative = "disposal/dis_5.ogg";

	public const string CharacterSelectRelative = "ui/char_select.mp3";

	public const string CylinderTickRelative = "ui/cylinder_tick.mp3";

	public const string MultiplayerPingRelative = "ui/aim_for_the_heart_ping.mp3";

	public const string ReloadOnceRelative = "reload/reload_once.mp3";

	public const string DeathRelative = "death/death.mp3";

	public const string TremorBurstRelative = "effects/tremor_burst.mp3";

	public const string TremorStaggerRelative = "effects/tremor_stagger.mp3";

	public const string PrecognitionOverheatRelative = "voice/precognition/overheat.mp3";

	public const string BossMusicRelative = "music/boss_cp9_1_2.mp3";

	public const string Act4RunMusicRelative = "music/act4_run.mp3";

	public const string AttackStart = "res://Valencina/audio/attack/atk1_1.mp3";

	public const string AttackHitOne = "res://Valencina/audio/attack/atk1_2.mp3";

	public const string AttackHitTwo = "res://Valencina/audio/attack/atk1_3.mp3";

	public const string Attack2Start = "res://Valencina/audio/attack/atk2_1.mp3";

	public const string Attack2HitOne = "res://Valencina/audio/attack/atk2_2.mp3";

	public const string Attack2HitTwo = "res://Valencina/audio/attack/atk2_3.mp3";

	public const string DisposalVoice = "res://Valencina/audio/disposal/voice.ogg";

	public const string DisposalVoiceAlt = "res://Valencina/audio/disposal/voice_2.mp3";

	public const string DisposalHitOne = "res://Valencina/audio/disposal/dis_1.ogg";

	public const string DisposalHitTwo = "res://Valencina/audio/disposal/dis_2.ogg";

	public const string DisposalHitThree = "res://Valencina/audio/disposal/dis_3.ogg";

	public const string DisposalHitFour = "res://Valencina/audio/disposal/dis_4.ogg";

	public const string DisposalHitFive = "res://Valencina/audio/disposal/dis_5.ogg";

	public const string CharacterSelect = "res://Valencina/audio/ui/char_select.mp3";

	public const string CylinderTick = "res://Valencina/audio/ui/cylinder_tick.mp3";

	public const string MultiplayerPing = "res://Valencina/audio/ui/aim_for_the_heart_ping.mp3";

	public const string ReloadOnce = "res://Valencina/audio/reload/reload_once.mp3";

	public const string Death = "res://Valencina/audio/death/death.mp3";

	public const string TremorBurst = "res://Valencina/audio/effects/tremor_burst.mp3";

	public const string TremorStagger = "res://Valencina/audio/effects/tremor_stagger.mp3";

	public const string PrecognitionOverheat = "res://Valencina/audio/voice/precognition/overheat.mp3";

	public const string BossMusic = "res://Valencina/audio/music/boss_cp9_1_2.mp3";

	public const string Act4RunMusic = "res://Valencina/audio/music/act4_run.mp3";

	private const string AudioRoot = "res://Valencina/audio";

	private const int MaxConcurrentLocalSfx = 10;

	private const float LocalSfxVolumeMultiplier = 1.28f;

	private const float TeammateSfxVolumeMultiplier = 0.38f;

	private const int MaxSfxStartsPerBurstWindow = 6;

	private const ulong BurstWindowMs = 120uL;

	private const ulong DefaultSameSfxCooldownMs = 45uL;

	private const ulong AttackStartSameSfxCooldownMs = 1045uL;

	private const ulong CharacterSelectCooldownMs = 180uL;

	private const ulong ReloadSameSfxCooldownMs = 90uL;

	private const ulong CylinderTickSameSfxCooldownMs = 38uL;

	private const ulong OverloadLogCooldownMs = 1200uL;

	private static readonly string[] SfxBusCandidates = new string[7] { "SFX", "Sfx", "Effects", "Effect", "Sound", "Sounds", "Master" };

	private static readonly string[] MusicBusCandidates = new string[5] { "Music", "BGM", "Bgm", "BgmMusic", "Master" };

	private static readonly string[] DisposalVoiceRelativePaths = new string[2] { "disposal/voice.ogg", "disposal/voice_2.mp3" };

	private static readonly List<SfxSlot> ActiveLocalSfx = new List<SfxSlot>();

	private static readonly Dictionary<string, ulong> LastLocalSfxStarts = new Dictionary<string, ulong>(StringComparer.Ordinal);

	private static string? _lastDisposalVoiceRelativePath;

	private static ulong _lastCharacterSelectStartedAt;

	private static ulong _burstWindowStartedAt;

	private static int _burstStarts;

	private static ulong _nextOverloadLogAt;

	private static string? _resolvedSfxBus;

	private static string? _resolvedMusicBus;

	private static readonly HashSet<string> LoggedBusResolutions = new HashSet<string>(StringComparer.Ordinal);

	private static StringName SfxBus => new StringName(ResolveBus(ref _resolvedSfxBus, "sfx", SfxBusCandidates));

	private static StringName MusicBus => new StringName(ResolveBus(ref _resolvedMusicBus, "music", MusicBusCandidates));

	public static AudioStreamPlayer? Play(string path, Node? anchor = null, float volume = 1f)
	{
		return PlaySfx(ToRelativeAudioPath(path), 0f, volume, 0f, 1f, anchor);
	}

	public static AudioStreamPlayer? PlayCharacterSelect(float volumeMult = 1f)
	{
		return PlaySfx("ui/char_select.mp3", 0f, volumeMult);
	}

	public static AudioStreamPlayer? PlayCharacterSelectOnce(float volumeMult = 1f)
	{
		ulong ticksMsec = Time.GetTicksMsec();
		if (ticksMsec >= _lastCharacterSelectStartedAt && ticksMsec - _lastCharacterSelectStartedAt < 180)
		{
			return null;
		}
		_lastCharacterSelectStartedAt = ticksMsec;
		return PlayCharacterSelect(volumeMult);
	}

	public static AudioStreamPlayer? PlayMultiplayerPing(Node? anchor = null, float volumeMult = 1f)
	{
		return PlaySfx("ui/aim_for_the_heart_ping.mp3", 0f, volumeMult, 0f, 1f, anchor);
	}

	public static string ResolveStreamingMusicPath(string path)
	{
		string text = ToRelativeAudioPath(path);
		foreach (string looseAudioFileCandidate in GetLooseAudioFileCandidates(text))
		{
			if (File.Exists(looseAudioFileCandidate))
			{
				return looseAudioFileCandidate;
			}
		}
		string text2 = ToAbsoluteAudioPath(text);
		MainFile.Logger.Warn($"[ValencinaAudio] Loose music file not found for '{text}'. Falling back to '{text2}', which may be rejected by RitsuLib FMOD streaming when packed as an imported resource.", 1);
		return text2;
	}

	public static bool ShouldPlayForPlayer(Player? player)
	{
		if (player == null)
		{
			return false;
		}
		if (!LocalContext.IsMe(player))
		{
			return ValencinaModConfig.EnableMultiplayerTeammateSfx;
		}
		return true;
	}

	public static float VolumeMultiplierForPlayer(Player? player)
	{
		if (player == null || LocalContext.IsMe(player))
		{
			return 1f;
		}
		return 0.38f;
	}

	public static AudioStreamPlayer? PlaySfx(string relativePath, float volume = 0f, float volumeMult = 1f, float pitchVariation = 0f, float basePitch = 1f, Node? fallbackAnchor = null)
	{
		relativePath = ToRelativeAudioPath(relativePath);
		bool highPriority = IsHighPrioritySfx(relativePath);
		if (!CanStartLocalSfx(relativePath, highPriority))
		{
			return null;
		}
		AudioStreamPlayer val = PlayFallback(ToAbsoluteAudioPath(relativePath), fallbackAnchor, volume, volumeMult * 1.28f, SfxBus, basePitch, autoFreeOnFinished: true, loop: false, registerAsLocalSfx: true, relativePath, highPriority);
		if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			return val;
		}
		return null;
	}

	public static AudioStreamPlayer? PlayAmbience(string relativePath, float volume = 0f, float volumeMult = 1f, float pitchVariation = 0f, float basePitch = 1f)
	{
		return PlayFallback(ToAbsoluteAudioPath(relativePath), null, volume, volumeMult, MusicBus, basePitch);
	}

	public static void PlayAttackSequence(Node anchor, float volumeMult = 1f)
	{
		PlaySfx("attack/atk1_1.mp3", 0f, volumeMult, 0f, 1f, anchor);
		PlayDelayedAsync("attack/atk1_2.mp3", anchor, 0.16, volumeMult);
		PlayDelayedAsync("attack/atk1_3.mp3", anchor, 0.32, volumeMult);
	}

	public static void PlayDisposalSequence(Node anchor, float volumeMult = 1f, float timelineSpeed = 1f)
	{
		timelineSpeed = Math.Max(0.01f, timelineSpeed);
		PlaySfx(PickNonRepeating(DisposalVoiceRelativePaths, ref _lastDisposalVoiceRelativePath), 0f, volumeMult, 0f, 1f, anchor);
		PlayDelayedAsync("disposal/dis_1.ogg", anchor, 0.82 / (double)timelineSpeed, volumeMult, timelineSpeed);
		PlayDelayedAsync("disposal/dis_2.ogg", anchor, 1.68 / (double)timelineSpeed, volumeMult, timelineSpeed);
		PlayDelayedAsync("disposal/dis_3.ogg", anchor, 1.365 / (double)timelineSpeed, volumeMult, timelineSpeed);
		PlayDelayedAsync("disposal/dis_4.ogg", anchor, 1.92 / (double)timelineSpeed, volumeMult, timelineSpeed);
		PlayDelayedAsync("disposal/dis_5.ogg", anchor, 3.6 / (double)timelineSpeed, volumeMult, timelineSpeed);
		PlayDelayedAsync("effects/tremor_burst.mp3", anchor, 4.25 / (double)timelineSpeed, volumeMult, timelineSpeed);
		PlayDelayedAsync("effects/tremor_burst.mp3", anchor, 4.85 / (double)timelineSpeed, volumeMult, timelineSpeed);
		PlayDelayedAsync("effects/tremor_burst.mp3", anchor, 5.3 / (double)timelineSpeed, volumeMult, timelineSpeed);
	}

	public static AudioStreamPlayer? PlayTremorBurst(Node? anchor = null)
	{
		return PlaySfx("effects/tremor_burst.mp3", 0f, 1f, 0f, 1f, anchor);
	}

	public static AudioStreamPlayer? PlayTremorStagger(Node? anchor = null)
	{
		return PlaySfx("effects/tremor_stagger.mp3", 0f, 1f, 0f, 1f, anchor);
	}

	public static void PlayAttack2Start(Node anchor, float volumeMult = 1f)
	{
		PlaySfx("attack/atk2_1.mp3", 0f, volumeMult, 0f, 1f, anchor);
	}

	public static void PlayAttack2HitOne(Node anchor, float volumeMult = 1f)
	{
		PlaySfx("attack/atk2_2.mp3", 0f, volumeMult, 0f, 1f, anchor);
	}

	public static void PlayAttack2HitTwo(Node anchor, float volumeMult = 1f)
	{
		PlaySfx("attack/atk2_3.mp3", 0f, volumeMult, 0f, 1f, anchor);
	}

	public static AudioStreamPlayer? PlayPrecognitionOverheat(Node? anchor = null)
	{
		return PlaySfx("voice/precognition/overheat.mp3", 0f, 1f, 0f, 1f, anchor);
	}

	public static AudioStreamPlayer? PlayVoice(string relativePath, float volumeMult = 1f, Node? fallbackAnchor = null)
	{
		return PlaySfx(relativePath, 0f, volumeMult, 0f, 1f, fallbackAnchor);
	}

	private static string PickNonRepeating(IReadOnlyList<string> relativePaths, ref string? lastRelativePath)
	{
		if (relativePaths.Count <= 1)
		{
			return lastRelativePath = ((relativePaths.Count == 0) ? string.Empty : relativePaths[0]);
		}
		string text;
		do
		{
			text = relativePaths[Random.Shared.Next(relativePaths.Count)];
		}
		while (string.Equals(text, lastRelativePath, StringComparison.Ordinal));
		lastRelativePath = text;
		return text;
	}

	public static void PlayCylinderTicks(Node anchor, int ticks)
	{
		int num = Math.Clamp(ticks, 0, 8);
		for (int i = 0; i < num; i++)
		{
			PlayDelayedAsync("ui/cylinder_tick.mp3", anchor, (double)i * 0.055);
		}
	}

	public static void RecoverTransientSfx(string reason)
	{
		StopAllTransientSfx(reason);
		ValencinaProbeLog.Warn("audio-sfx-recover", "Transient Valencina SFX reset. reason=" + reason);
	}

	public static void StopAllTransientSfx(string reason)
	{
		CleanupActiveLocalSfx();
		StopAllLocalSfx();
		LastLocalSfxStarts.Clear();
		_burstWindowStartedAt = 0uL;
		_burstStarts = 0;
		_nextOverloadLogAt = 0uL;
		ValencinaProbeLog.Info("audio-sfx-stop-all", "Stopped all transient Valencina SFX. reason=" + reason, 20);
	}

	private static async Task PlayDelayedAsync(string relativePath, Node anchor, double seconds, float volumeMult = 1f, float basePitch = 1f)
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)anchor))
		{
			return;
		}
		SceneTree tree = anchor.GetTree();
		if (tree != null)
		{
			SceneTreeTimer val = tree.CreateTimer(seconds, true, false, false);
			await ((GodotObject)anchor).ToSignal((GodotObject)(object)val, SignalName.Timeout);
			if (GodotObject.IsInstanceValid((GodotObject)(object)anchor))
			{
				PlaySfx(relativePath, 0f, volumeMult, 0f, basePitch, anchor);
			}
		}
	}

	private static bool CanStartLocalSfx(string relativePath, bool highPriority)
	{
		CleanupActiveLocalSfx();
		ulong ticksMsec = Time.GetTicksMsec();
		ulong num = SamePathCooldownFor(relativePath);
		if (!highPriority && LastLocalSfxStarts.TryGetValue(relativePath, out var value) && ticksMsec >= value && ticksMsec - value < num)
		{
			return false;
		}
		if (ticksMsec < _burstWindowStartedAt || ticksMsec - _burstWindowStartedAt > 120)
		{
			_burstWindowStartedAt = ticksMsec;
			_burstStarts = 0;
		}
		if (!highPriority && _burstStarts >= 6)
		{
			LogSfxOverloadOnce(ticksMsec, "burst limit hit for " + relativePath);
			return false;
		}
		if (ActiveLocalSfx.Count >= 10 && (!highPriority || !StopOldestLocalSfx()))
		{
			LogSfxOverloadOnce(ticksMsec, $"concurrent limit hit for {relativePath}; active={ActiveLocalSfx.Count}");
			return false;
		}
		_burstStarts++;
		LastLocalSfxStarts[relativePath] = ticksMsec;
		return true;
	}

	private static ulong SamePathCooldownFor(string relativePath)
	{
		if (string.Equals(relativePath, "reload/reload_once.mp3", StringComparison.Ordinal))
		{
			return 90uL;
		}
		if (string.Equals(relativePath, "ui/cylinder_tick.mp3", StringComparison.Ordinal))
		{
			return 38uL;
		}
		if (string.Equals(relativePath, "attack/atk1_1.mp3", StringComparison.Ordinal))
		{
			return 1045uL;
		}
		return 45uL;
	}

	private static bool IsHighPrioritySfx(string relativePath)
	{
		if (!relativePath.StartsWith("voice/", StringComparison.Ordinal) && !string.Equals(relativePath, "ui/char_select.mp3", StringComparison.Ordinal) && !string.Equals(relativePath, "ui/aim_for_the_heart_ping.mp3", StringComparison.Ordinal) && !string.Equals(relativePath, "disposal/voice.ogg", StringComparison.Ordinal) && !string.Equals(relativePath, "disposal/voice_2.mp3", StringComparison.Ordinal) && !string.Equals(relativePath, "death/death.mp3", StringComparison.Ordinal) && !string.Equals(relativePath, "effects/tremor_burst.mp3", StringComparison.Ordinal) && !string.Equals(relativePath, "effects/tremor_stagger.mp3", StringComparison.Ordinal))
		{
			return string.Equals(relativePath, "voice/precognition/overheat.mp3", StringComparison.Ordinal);
		}
		return true;
	}

	private static void LogSfxOverloadOnce(ulong now, string message)
	{
		if (now >= _nextOverloadLogAt)
		{
			_nextOverloadLogAt = now + 1200;
			ValencinaProbeLog.Warn("audio-sfx-overload-throttle", "Valencina SFX throttled: " + message + ".");
		}
	}

	private static bool StopOldestLocalSfx()
	{
		CleanupActiveLocalSfx();
		SfxSlot sfxSlot = (from active in ActiveLocalSfx
			orderby active.HighPriority, active.StartedAt
			select active).FirstOrDefault();
		if (sfxSlot == null || !GodotObject.IsInstanceValid((GodotObject)(object)sfxSlot.Player))
		{
			return false;
		}
		try
		{
			sfxSlot.Player.Stop();
			((Node)sfxSlot.Player).QueueFree();
		}
		catch
		{
		}
		ActiveLocalSfx.Remove(sfxSlot);
		return true;
	}

	private static void RegisterLocalSfxPlayer(AudioStreamPlayer player, string logicalPath, bool highPriority)
	{
		CleanupActiveLocalSfx();
		SfxSlot slot = new SfxSlot(player, logicalPath, Time.GetTicksMsec(), highPriority);
		ActiveLocalSfx.Add(slot);
		player.Finished += delegate
		{
			ActiveLocalSfx.Remove(slot);
		};
		((Node)player).TreeExiting += delegate
		{
			ActiveLocalSfx.Remove(slot);
		};
	}

	private static void CleanupActiveLocalSfx()
	{
		ActiveLocalSfx.RemoveAll((SfxSlot slot) => !GodotObject.IsInstanceValid((GodotObject)(object)slot.Player) || !slot.Player.Playing);
	}

	private static void StopAllLocalSfx()
	{
		for (int num = ActiveLocalSfx.Count - 1; num >= 0; num--)
		{
			AudioStreamPlayer player = ActiveLocalSfx[num].Player;
			if (GodotObject.IsInstanceValid((GodotObject)(object)player))
			{
				try
				{
					player.Stop();
					((Node)player).QueueFree();
				}
				catch
				{
				}
			}
		}
		ActiveLocalSfx.Clear();
	}

	private static AudioStreamPlayer? PlayFallback(string path, Node? anchor, float volumeDb, float volumeMult, StringName bus, float basePitch, bool autoFreeOnFinished = true, bool loop = false, bool registerAsLocalSfx = false, string? logicalPath = null, bool highPriority = false)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Expected O, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		AudioStream val = ResourceLoader.Load<AudioStream>(path, string.Empty, (CacheMode)1);
		if (val == null)
		{
			ValencinaProbeLog.Warn("audio-fallback-missing-stream", "Fallback audio stream missing: " + path);
			return null;
		}
		if (loop)
		{
			ConfigureLoop(val, path);
		}
		object obj2;
		if (!GodotObject.IsInstanceValid((GodotObject)(object)anchor))
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			MainLoop obj = ((mainLoop is SceneTree) ? mainLoop : null);
			obj2 = ((obj != null) ? ((SceneTree)obj).Root : null);
		}
		else
		{
			obj2 = anchor;
		}
		Node val2 = (Node)obj2;
		if (val2 == null)
		{
			ValencinaProbeLog.Warn("audio-fallback-missing-parent", "Fallback audio parent missing: " + path);
			return null;
		}
		AudioStreamPlayer val3 = new AudioStreamPlayer
		{
			Stream = val,
			Bus = bus,
			VolumeDb = SanitizeVolumeDb(volumeDb) + LinearToDbSafe(SanitizeVolumeMult(volumeMult)),
			PitchScale = SanitizePitch(basePitch),
			ProcessMode = (ProcessModeEnum)3
		};
		((GodotObject)val2).CallDeferred(MethodName.AddChild, (Variant[])(object)new Variant[1] { Variant.op_Implicit((GodotObject)(object)val3) });
		if (autoFreeOnFinished)
		{
			val3.Finished += ((Node)val3).QueueFree;
		}
		if (registerAsLocalSfx)
		{
			RegisterLocalSfxPlayer(val3, logicalPath ?? path, highPriority);
		}
		PlayWhenInsideTree(val3);
		return val3;
	}

	private static async Task PlayWhenInsideTree(AudioStreamPlayer player)
	{
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree tree = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		for (int i = 0; i < 8; i++)
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)player))
			{
				break;
			}
			if (((Node)player).IsInsideTree())
			{
				if (player.Stream != null && !player.StreamPaused)
				{
					player.Play(0f);
				}
				break;
			}
			if (tree == null)
			{
				break;
			}
			await ((GodotObject)tree).ToSignal((GodotObject)(object)tree, SignalName.ProcessFrame);
		}
	}

	private static string ResolveBus(ref string? cachedBus, string kind, IReadOnlyList<string> candidates)
	{
		if (!string.IsNullOrWhiteSpace(cachedBus) && BusExists(cachedBus))
		{
			return cachedBus;
		}
		foreach (string candidate in candidates)
		{
			if (BusExists(candidate))
			{
				cachedBus = candidate;
				LogBusResolution(kind, candidate, candidate == "Master");
				return candidate;
			}
		}
		cachedBus = "Master";
		LogBusResolution(kind, cachedBus, fallback: true);
		return cachedBus;
	}

	private static bool BusExists(string busName)
	{
		try
		{
			return AudioServer.GetBusIndex(StringName.op_Implicit(busName)) >= 0;
		}
		catch
		{
			return string.Equals(busName, "Master", StringComparison.Ordinal);
		}
	}

	private static void LogBusResolution(string kind, string busName, bool fallback)
	{
		string item = $"{kind}:{busName}:{fallback}";
		if (LoggedBusResolutions.Add(item))
		{
			if (fallback && !string.Equals(busName, "Master", StringComparison.Ordinal))
			{
				MainFile.Logger.Warn($"[ValencinaAudio] {kind} audio bus resolved to fallback '{busName}'.", 1);
			}
			else
			{
				MainFile.Logger.Info($"[ValencinaAudio] {kind} audio bus resolved to '{busName}'.", 1);
			}
		}
	}

	private static void ConfigureLoop(AudioStream stream, string path)
	{
		try
		{
			AudioStreamMP3 val = (AudioStreamMP3)(object)((stream is AudioStreamMP3) ? stream : null);
			if (val != null)
			{
				val.Loop = true;
				return;
			}
			AudioStreamOggVorbis val2 = (AudioStreamOggVorbis)(object)((stream is AudioStreamOggVorbis) ? stream : null);
			if (val2 != null)
			{
				val2.Loop = true;
			}
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[ValencinaAudio] Failed to configure loop for '" + path + "': " + ex.Message, 1);
		}
	}

	private static float SanitizeVolumeDb(float volumeDb)
	{
		if (float.IsNaN(volumeDb) || float.IsInfinity(volumeDb))
		{
			return 0f;
		}
		return Mathf.Clamp(volumeDb, -80f, 12f);
	}

	private static float SanitizeVolumeMult(float volumeMult)
	{
		if (float.IsNaN(volumeMult) || float.IsInfinity(volumeMult) || volumeMult <= 0f)
		{
			return 1f;
		}
		return Mathf.Clamp(volumeMult, 0.001f, 4f);
	}

	private static float SanitizePitch(float pitch)
	{
		if (float.IsNaN(pitch) || float.IsInfinity(pitch) || pitch <= 0f)
		{
			return 1f;
		}
		return Mathf.Clamp(pitch, 0.01f, 4f);
	}

	private static float LinearToDbSafe(float linear)
	{
		if (linear <= 0f)
		{
			return -80f;
		}
		return Mathf.LinearToDb(linear);
	}

	private static string ToRelativeAudioPath(string path)
	{
		if (!path.StartsWith("res://Valencina/audio/", StringComparison.Ordinal))
		{
			return path;
		}
		int length = "res://Valencina/audio/".Length;
		return path.Substring(length, path.Length - length);
	}

	private static string ToAbsoluteAudioPath(string path)
	{
		if (!path.StartsWith("res://", StringComparison.Ordinal))
		{
			return "res://Valencina/audio/" + path;
		}
		return path;
	}

	private static IEnumerable<string> GetLooseAudioFileCandidates(string relativePath)
	{
		string normalizedRelativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
		string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		if (!string.IsNullOrWhiteSpace(directoryName))
		{
			yield return Path.Combine(directoryName, "audio", normalizedRelativePath);
		}
		_003C_003Ey__InlineArray5<string> buffer = default(_003C_003Ey__InlineArray5<string>);
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray5<string>, string>(ref buffer, 0) = AppContext.BaseDirectory;
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray5<string>, string>(ref buffer, 1) = "mods";
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray5<string>, string>(ref buffer, 2) = "Valencina";
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray5<string>, string>(ref buffer, 3) = "audio";
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray5<string>, string>(ref buffer, 4) = normalizedRelativePath;
		yield return Path.Combine(global::_003CPrivateImplementationDetails_003E.InlineArrayAsReadOnlySpan<_003C_003Ey__InlineArray5<string>, string>(in buffer, 5));
	}
}
