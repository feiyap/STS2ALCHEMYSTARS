using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace Valencina.ValencinaCode.Utils;

public static class ValencinaVoiceSfx
{
	private sealed class VoiceTurnState
	{
		private CombatSide? _side;

		public bool DodgeSuccessPlayed { get; set; }

		public bool DodgeFailPlayed { get; set; }

		public int AttackVoicesPlayed { get; set; }

		public ulong NextAttackVoiceAllowedAt { get; set; }

		public void ResetIfSideChanged(CombatSide side)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			if (_side != (CombatSide?)side)
			{
				ResetForSide(side);
			}
		}

		public void ResetForSide(CombatSide side)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			_side = side;
			DodgeSuccessPlayed = false;
			DodgeFailPlayed = false;
			AttackVoicesPlayed = 0;
			NextAttackVoiceAllowedAt = 0uL;
		}
	}

	private const float AttackVoiceChance = 0.33f;

	private const float DodgeSuccessVoiceChance = 0.45f;

	private const float DodgeFailVoiceChance = 0.35f;

	private const float VoiceVolumeMultiplier = 1.42f;

	private const int MaxAttackVoicesPerTurn = 3;

	private const ulong MinAttackVoiceGapMs = 420uL;

	private static readonly string[] DodgeSuccessRelative = new string[1] { "voice/dodge_success/success_1.mp3" };

	private static readonly string[] DodgeFailRelative = new string[5] { "voice/dodge_fail/fail_1.mp3", "voice/dodge_fail/fail_2.mp3", "voice/dodge_fail/fail_3.mp3", "voice/dodge_fail/fail_4.mp3", "voice/dodge_fail/fail_5.mp3" };

	private static readonly string[] AttackRelative = new string[13]
	{
		"voice/attack/attack_1.mp3", "voice/attack/attack_2.mp3", "voice/attack/attack_3.mp3", "voice/attack/attack_4.mp3", "voice/attack/attack_5.mp3", "voice/attack/attack_6.mp3", "voice/attack/attack_7.mp3", "voice/attack/9SV-BAT8-BR2-1-03.wav", "voice/attack/9SV-BAT8-BR3-1-02.wav", "voice/attack/9SV-BAT8-BR3-2-01.wav",
		"voice/attack/9SV-BAT8-BR4-1-01.wav", "voice/attack/9SV-BAT8-BR4-4-01.wav", "voice/attack/9SV-BAT8-BR5-2-01.wav"
	};

	private static readonly string[] PrecognitionOverheatRelative = new string[1] { "voice/precognition/overheat.mp3" };

	private static readonly ConditionalWeakTable<Creature, VoiceTurnState> TurnStates = new ConditionalWeakTable<Creature, VoiceTurnState>();

	private static AudioStreamPlayer? _activeVoice;

	private static string? _lastVoiceRelativePath;

	public static IEnumerable<string> AssetPaths => from path in DodgeSuccessRelative.Concat(DodgeFailRelative).Concat(AttackRelative).Concat(PrecognitionOverheatRelative)
		select "res://Valencina/audio/" + path;

	public static void ResetTurn(Creature? owner)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (owner != null)
		{
			VoiceTurnState orCreateValue = TurnStates.GetOrCreateValue(owner);
			ICombatState combatState = owner.CombatState;
			orCreateValue.ResetForSide((combatState != null) ? combatState.CurrentSide : owner.Side);
		}
	}

	public static void TryPlayDodgeSuccess(Creature? owner, Node? anchor = null)
	{
		if (CanUseLocalVoice(owner, out VoiceTurnState state, out float volumeScale) && !state.DodgeSuccessPlayed)
		{
			state.DodgeSuccessPlayed = true;
			if (Random.Shared.NextDouble() < 0.44999998807907104)
			{
				TryPlayRandom(DodgeSuccessRelative, anchor, interruptCurrent: true, volumeScale);
			}
		}
	}

	public static void TryPlayDodgeFail(Creature? owner, Node? anchor = null)
	{
		if (CanUseLocalVoice(owner, out VoiceTurnState state, out float volumeScale) && !state.DodgeFailPlayed)
		{
			state.DodgeFailPlayed = true;
			if (Random.Shared.NextDouble() < 0.3499999940395355)
			{
				TryPlayRandom(DodgeFailRelative, anchor, interruptCurrent: true, volumeScale);
			}
		}
	}

	public static void TryPlayPrecognitionOverheat(Creature? owner, Node? anchor = null)
	{
		if (CanUseLocalVoice(owner, out VoiceTurnState _, out float volumeScale))
		{
			TryPlayRandom(PrecognitionOverheatRelative, anchor, interruptCurrent: true, volumeScale);
		}
	}

	public static void TryPlayAttackVoice(Creature? owner, Node? anchor = null)
	{
		if (CanUseLocalVoice(owner, out VoiceTurnState state, out float volumeScale) && state.AttackVoicesPlayed < 3 && Time.GetTicksMsec() >= state.NextAttackVoiceAllowedAt && !(Random.Shared.NextDouble() >= 0.33000001311302185) && !IsVoicePlaying() && TryPlayRandom(AttackRelative, anchor, interruptCurrent: false, volumeScale))
		{
			state.AttackVoicesPlayed++;
			state.NextAttackVoiceAllowedAt = Time.GetTicksMsec() + 420;
		}
	}

	public static void StopAll(string reason)
	{
		StopActiveVoice();
		ValencinaProbeLog.Info("audio-voice-stop-all", "Stopped Valencina voice playback. reason=" + reason, 20);
	}

	private static bool CanUseLocalVoice(Creature? owner, out VoiceTurnState state, out float volumeScale)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		state = null;
		volumeScale = 1f;
		if (((owner != null) ? owner.Player : null) == null || !ValencinaLocalSfx.ShouldPlayForPlayer(owner.Player))
		{
			return false;
		}
		volumeScale = ValencinaLocalSfx.VolumeMultiplierForPlayer(owner.Player);
		state = TurnStates.GetOrCreateValue(owner);
		VoiceTurnState obj = state;
		ICombatState combatState = owner.CombatState;
		obj.ResetIfSideChanged((combatState != null) ? combatState.CurrentSide : owner.Side);
		return true;
	}

	private static bool TryPlayRandom(IReadOnlyList<string> relativePaths, Node? anchor, bool interruptCurrent, float volumeScale)
	{
		if (relativePaths.Count == 0)
		{
			return false;
		}
		if (interruptCurrent)
		{
			StopActiveVoice();
		}
		else if (IsVoicePlaying())
		{
			return false;
		}
		string text = PickNonRepeating(relativePaths);
		AudioStreamPlayer player = ValencinaLocalSfx.PlayVoice(text, 1.42f * volumeScale, anchor);
		if (player == null || !GodotObject.IsInstanceValid((GodotObject)(object)player))
		{
			return false;
		}
		_activeVoice = player;
		_lastVoiceRelativePath = text;
		player.Finished += delegate
		{
			if (_activeVoice == player)
			{
				_activeVoice = null;
			}
		};
		((Node)player).TreeExiting += delegate
		{
			if (_activeVoice == player)
			{
				_activeVoice = null;
			}
		};
		return true;
	}

	private static string PickNonRepeating(IReadOnlyList<string> relativePaths)
	{
		if (relativePaths.Count <= 1)
		{
			return relativePaths[0];
		}
		string text;
		do
		{
			text = relativePaths[Random.Shared.Next(relativePaths.Count)];
		}
		while (string.Equals(text, _lastVoiceRelativePath, StringComparison.Ordinal));
		return text;
	}

	private static void StopActiveVoice()
	{
		if (_activeVoice == null || !GodotObject.IsInstanceValid((GodotObject)(object)_activeVoice))
		{
			_activeVoice = null;
			return;
		}
		try
		{
			_activeVoice.Stop();
			((Node)_activeVoice).QueueFree();
		}
		catch
		{
		}
		finally
		{
			_activeVoice = null;
		}
	}

	private static bool IsVoicePlaying()
	{
		if (_activeVoice == null || !GodotObject.IsInstanceValid((GodotObject)(object)_activeVoice))
		{
			_activeVoice = null;
			return false;
		}
		if (!_activeVoice.Playing)
		{
			_activeVoice = null;
			return false;
		}
		return true;
	}
}
