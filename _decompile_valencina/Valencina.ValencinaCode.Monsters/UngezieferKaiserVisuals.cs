using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Monsters;

[ScriptPath("res://ValencinaCode/Monsters/UngezieferKaiserVisuals.cs")]
public class UngezieferKaiserVisuals : NCreatureVisuals
{
	private readonly record struct AttackFrame(string FileName, double Time, Vector2 Offset);

	private readonly record struct ShakeEvent(double Time, Vector2 Offset);

	public class MethodName : MethodName
	{
		public static readonly StringName _Ready = StringName.op_Implicit("_Ready");

		public static readonly StringName PlayAttack = StringName.op_Implicit("PlayAttack");

		public static readonly StringName PlayTurnStartVoice = StringName.op_Implicit("PlayTurnStartVoice");

		public static readonly StringName EnsureAttackSprite = StringName.op_Implicit("EnsureAttackSprite");

		public static readonly StringName LoadAttackFrames = StringName.op_Implicit("LoadAttackFrames");

		public static readonly StringName BuildAnimationLibrary = StringName.op_Implicit("BuildAnimationLibrary");

		public static readonly StringName BuildIdleAnimation = StringName.op_Implicit("BuildIdleAnimation");

		public static readonly StringName ResetToIdleVisualState = StringName.op_Implicit("ResetToIdleVisualState");

		public static readonly StringName AddDiscreteTrack = StringName.op_Implicit("AddDiscreteTrack");

		public static readonly StringName AttackTexture = StringName.op_Implicit("AttackTexture");

		public static readonly StringName GetRightAlignedPosition = StringName.op_Implicit("GetRightAlignedPosition");

		public static readonly StringName ConnectAnimationFinished = StringName.op_Implicit("ConnectAnimationFinished");

		public static readonly StringName OnAnimationFinished = StringName.op_Implicit("OnAnimationFinished");

		public static readonly StringName IsVoicePlaying = StringName.op_Implicit("IsVoicePlaying");
	}

	public class PropertyName : PropertyName
	{
		public static readonly StringName _idleFrames = StringName.op_Implicit("_idleFrames");

		public static readonly StringName _idle = StringName.op_Implicit("_idle");

		public static readonly StringName _attack = StringName.op_Implicit("_attack");

		public static readonly StringName _animationPlayer = StringName.op_Implicit("_animationPlayer");

		public static readonly StringName _activeVoicePlayer = StringName.op_Implicit("_activeVoicePlayer");

		public static readonly StringName _lastVoiceRelativePath = StringName.op_Implicit("_lastVoiceRelativePath");

		public static readonly StringName _animationFinishedConnected = StringName.op_Implicit("_animationFinishedConnected");
	}

	public class SignalName : SignalName
	{
	}

	private const string IdleFramePathPrefix = "res://Valencina/images/monsters/ungeziefer_kaiser/idle/idle_";

	private const string AttackFramePathPrefix = "res://Valencina/images/monsters/ungeziefer_kaiser/attack/";

	private const int IdleFrameCount = 60;

	private const double FramesPerSecond = 12.0;

	private const float Skill1Length = 1.48f;

	private const float Skill2Length = 1.58f;

	private const float Skill4Length = 1.52f;

	private static readonly Vector2 LightShakeA = new Vector2(2f, -1f);

	private static readonly Vector2 LightShakeB = new Vector2(-2f, 1f);

	private static readonly Vector2 HeavyShakeA = new Vector2(10f, -4f);

	private static readonly Vector2 HeavyShakeB = new Vector2(-8f, 3f);

	private static readonly Vector2 HeavyShakeC = new Vector2(5f, -2f);

	private static readonly Vector2 Skill4Lift = new Vector2(0f, -10f);

	private readonly Texture2D?[] _idleFrames = (Texture2D?[])(object)new Texture2D[60];

	private readonly Dictionary<string, Texture2D?> _attackFrames = new Dictionary<string, Texture2D>(StringComparer.Ordinal);

	private Sprite2D? _idle;

	private Sprite2D? _attack;

	private AnimationPlayer? _animationPlayer;

	private AudioStreamPlayer? _activeVoicePlayer;

	private string? _lastVoiceRelativePath;

	private bool _animationFinishedConnected;

	public override void _Ready()
	{
		((NCreatureVisuals)this)._Ready();
		_idle = ((Node)this).GetNodeOrNull<Sprite2D>(NodePath.op_Implicit("Visuals/Idle"));
		_attack = EnsureAttackSprite();
		_animationPlayer = ((Node)this).GetNodeOrNull<AnimationPlayer>(NodePath.op_Implicit("AnimationPlayer"));
		if (_idle == null)
		{
			MainFile.Logger.Warn("[UngezieferKaiserVisuals] Missing Visuals/Idle sprite.", 1);
			return;
		}
		for (int i = 0; i < 60; i++)
		{
			string text = "res://Valencina/images/monsters/ungeziefer_kaiser/idle/idle_" + i.ToString("D3") + ".png";
			_idleFrames[i] = ResourceLoader.Load<Texture2D>(text, string.Empty, (CacheMode)1);
		}
		if (_idleFrames[0] != null)
		{
			_idle.Texture = _idleFrames[0];
		}
		LoadAttackFrames();
		if (_animationPlayer == null)
		{
			MainFile.Logger.Warn("[UngezieferKaiserVisuals] Missing AnimationPlayer node.", 1);
			return;
		}
		BuildAnimationLibrary();
		ConnectAnimationFinished();
		ResetToIdleVisualState();
		_animationPlayer.Play(StringName.op_Implicit("idle"), -1.0, 1f, false);
	}

	public void PlayAttack(int hitCount)
	{
		if (_animationPlayer != null)
		{
			PlayVoiceSfx(UngezieferKaiserAssets.AttackVoiceRelativePaths, 1.18f);
			if (((AnimationMixer)_animationPlayer).HasAnimation(StringName.op_Implicit("attack_skill4")))
			{
				_animationPlayer.Stop(false);
				_animationPlayer.Play(StringName.op_Implicit("attack_skill4"), -1.0, 1f, false);
			}
		}
	}

	public void PlayTurnStartVoice()
	{
		PlayVoiceSfx(UngezieferKaiserAssets.TurnVoiceRelativePaths, 1.05f);
	}

	private Sprite2D? EnsureAttackSprite()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		Node nodeOrNull = ((Node)this).GetNodeOrNull<Node>(NodePath.op_Implicit("Visuals"));
		if (nodeOrNull == null)
		{
			return null;
		}
		Sprite2D nodeOrNull2 = nodeOrNull.GetNodeOrNull<Sprite2D>(NodePath.op_Implicit("Attack"));
		if (nodeOrNull2 != null)
		{
			return nodeOrNull2;
		}
		Sprite2D val = new Sprite2D
		{
			Name = StringName.op_Implicit("Attack"),
			Visible = false
		};
		Sprite2D? idle = _idle;
		((Node2D)val).Position = (Vector2)((idle != null) ? ((Node2D)idle).Position : new Vector2(-6f, -295f));
		Sprite2D? idle2 = _idle;
		((Node2D)val).Scale = (Vector2)((idle2 != null) ? ((Node2D)idle2).Scale : new Vector2(0.62f, 0.64f));
		Sprite2D val2 = val;
		nodeOrNull.AddChild((Node)(object)val2, false, (InternalMode)0);
		return val2;
	}

	private void LoadAttackFrames()
	{
		string[] array = new string[24]
		{
			"skill1_2.png", "skill1_3.png", "skill1_4.png", "skill1_5.png", "skill1_6.png", "skill1_7.png", "skill1_8.png", "skill1_9.png", "skill1_10.png", "skill1_11.png",
			"skill2_2.png", "skill2_3.png", "skill2_4.png", "skill2_5.png", "skill2_6.png", "skill2_7.png", "skill2_8.png", "skill2_9.png", "skill4_2.png", "skill4_3.png",
			"skill4_4.png", "skill4_5.png", "skill4_6.png", "skill4_7~9.png"
		};
		foreach (string text in array)
		{
			_attackFrames[text] = ResourceLoader.Load<Texture2D>("res://Valencina/images/monsters/ungeziefer_kaiser/attack/" + text, string.Empty, (CacheMode)1);
		}
	}

	private void BuildAnimationLibrary()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Expected O, but got Unknown
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		if (_animationPlayer != null)
		{
			AnimationLibrary val = new AnimationLibrary();
			val.AddAnimation(StringName.op_Implicit("idle"), BuildIdleAnimation());
			val.AddAnimation(StringName.op_Implicit("attack_skill4"), GetExistingOrBuild("attack_skill4", () => BuildAttackAnimation("skill4", 1.52f, new _003C_003Ez__ReadOnlyArray<AttackFrame>(new AttackFrame[8]
			{
				new AttackFrame("skill4_2.png", 0.0, Skill4Lift),
				new AttackFrame("skill4_3.png", 0.08, Skill4Lift),
				new AttackFrame("skill4_4.png", 0.19, Skill4Lift),
				new AttackFrame("skill4_5.png", 0.34, Skill4Lift),
				new AttackFrame("skill4_6.png", 0.54, Skill4Lift),
				new AttackFrame("skill4_7~9.png", 0.8, Skill4Lift + LightShakeA),
				new AttackFrame("skill4_7~9.png", 1.05, Skill4Lift + LightShakeB),
				new AttackFrame("skill4_7~9.png", 1.33, Skill4Lift)
			}), new ShakeEvent(0.42, HeavyShakeA), new ShakeEvent(0.47, HeavyShakeB), new ShakeEvent(0.52, HeavyShakeC))));
			if (((AnimationMixer)_animationPlayer).HasAnimationLibrary(StringName.op_Implicit(string.Empty)))
			{
				((AnimationMixer)_animationPlayer).RemoveAnimationLibrary(StringName.op_Implicit(string.Empty));
			}
			((AnimationMixer)_animationPlayer).AddAnimationLibrary(StringName.op_Implicit(string.Empty), val);
		}
	}

	private Animation GetExistingOrBuild(string animationName, Func<Animation> buildAnimation)
	{
		if (_animationPlayer != null && ((AnimationMixer)_animationPlayer).HasAnimation(StringName.op_Implicit(animationName)))
		{
			return ((AnimationMixer)_animationPlayer).GetAnimation(StringName.op_Implicit(animationName));
		}
		return buildAnimation();
	}

	private Animation BuildIdleAnimation()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		Animation val = new Animation
		{
			ResourceName = "idle",
			Length = 5f,
			LoopMode = (LoopModeEnum)1
		};
		int num = AddDiscreteTrack(val, "Visuals/Idle:visible");
		int num2 = AddDiscreteTrack(val, "Visuals/Attack:visible");
		int num3 = AddDiscreteTrack(val, "Visuals:position");
		int num4 = val.AddTrack((TrackType)0, -1);
		val.TrackSetPath(num4, new NodePath("Visuals/Idle:texture"));
		val.ValueTrackSetUpdateMode(num4, (UpdateMode)1);
		val.TrackInsertKey(num, 0.0, Variant.op_Implicit(true), 1f);
		val.TrackInsertKey(num2, 0.0, Variant.op_Implicit(false), 1f);
		val.TrackInsertKey(num3, 0.0, Variant.op_Implicit(Vector2.Zero), 1f);
		for (int i = 0; i < 60; i++)
		{
			Texture2D val2 = _idleFrames[i];
			if (val2 != null)
			{
				val.TrackInsertKey(num4, (double)i / 12.0, Variant.op_Implicit((GodotObject)(object)val2), 1f);
			}
		}
		return val;
	}

	private void ResetToIdleVisualState()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (_idle != null)
		{
			((CanvasItem)_idle).Visible = true;
		}
		if (_attack != null)
		{
			((CanvasItem)_attack).Visible = false;
		}
		Node2D nodeOrNull = ((Node)this).GetNodeOrNull<Node2D>(NodePath.op_Implicit("Visuals"));
		if (nodeOrNull != null)
		{
			nodeOrNull.SetPosition(Vector2.Zero);
		}
	}

	private Animation BuildAttackAnimation(string name, float length, IReadOnlyList<AttackFrame> frames, params ShakeEvent[] shakeEvents)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		Animation val = new Animation
		{
			ResourceName = name,
			Length = length,
			LoopMode = (LoopModeEnum)0
		};
		int num = AddDiscreteTrack(val, "Visuals/Idle:visible");
		int num2 = AddDiscreteTrack(val, "Visuals/Attack:visible");
		int num3 = AddDiscreteTrack(val, "Visuals/Attack:texture");
		int num4 = val.AddTrack((TrackType)0, -1);
		val.TrackSetPath(num4, new NodePath("Visuals/Attack:position"));
		val.ValueTrackSetUpdateMode(num4, (UpdateMode)1);
		int num5 = AddDiscreteTrack(val, "Visuals/Attack:scale");
		Sprite2D? idle = _idle;
		Vector2 val2 = (Vector2)((idle != null) ? ((Node2D)idle).Scale : new Vector2(0.62f, 0.64f));
		val.TrackInsertKey(num, 0.0, Variant.op_Implicit(false), 1f);
		val.TrackInsertKey(num2, 0.0, Variant.op_Implicit(true), 1f);
		foreach (AttackFrame frame2 in frames)
		{
			Texture2D val3 = AttackTexture(frame2.FileName);
			Vector2 val4 = GetRightAlignedPosition(val3) + frame2.Offset;
			if (val3 != null)
			{
				val.TrackInsertKey(num3, frame2.Time, Variant.op_Implicit((GodotObject)(object)val3), 1f);
			}
			val.TrackInsertKey(num4, frame2.Time, Variant.op_Implicit(val4), 1f);
			val.TrackInsertKey(num5, frame2.Time, Variant.op_Implicit(val2), 1f);
		}
		for (int i = 0; i < shakeEvents.Length; i++)
		{
			ShakeEvent shakeEvent = shakeEvents[i];
			Texture2D frame = ((frames.Count > 0) ? AttackTexture(FindFrameAtOrBefore(frames, shakeEvent.Time).FileName) : null);
			val.TrackInsertKey(num4, shakeEvent.Time, Variant.op_Implicit(GetRightAlignedPosition(frame) + Skill4Lift + shakeEvent.Offset), 1f);
		}
		double num6 = Math.Max(0.0, (double)length - 0.05);
		val.TrackInsertKey(num2, num6, Variant.op_Implicit(false), 1f);
		val.TrackInsertKey(num, num6, Variant.op_Implicit(true), 1f);
		Sprite2D? attack = _attack;
		_003F val5;
		if (attack == null)
		{
			Sprite2D? idle2 = _idle;
			val5 = ((idle2 != null) ? ((Node2D)idle2).Position : Vector2.Zero);
		}
		else
		{
			val5 = ((Node2D)attack).Position;
		}
		val.TrackInsertKey(num4, num6, Variant.op_Implicit((Vector2)val5), 1f);
		val.TrackInsertKey(num5, num6, Variant.op_Implicit(val2), 1f);
		return val;
	}

	private static int AddDiscreteTrack(Animation animation, string path)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		int num = animation.AddTrack((TrackType)0, -1);
		animation.TrackSetPath(num, new NodePath(path));
		animation.ValueTrackSetUpdateMode(num, (UpdateMode)1);
		return num;
	}

	private Texture2D? AttackTexture(string name)
	{
		if (!_attackFrames.TryGetValue(name, out Texture2D value))
		{
			return null;
		}
		return value;
	}

	private Vector2 GetRightAlignedPosition(Texture2D? frame)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		Sprite2D? idle = _idle;
		Vector2 val = (Vector2)((idle != null) ? ((Node2D)idle).Position : new Vector2(-6f, -295f));
		Sprite2D? idle2 = _idle;
		Vector2 val2 = (Vector2)((idle2 != null) ? ((Node2D)idle2).Scale : new Vector2(0.62f, 0.64f));
		object obj = _idleFrames[0];
		if (obj == null)
		{
			Sprite2D? idle3 = _idle;
			obj = ((idle3 != null) ? idle3.Texture : null);
		}
		Texture2D val3 = (Texture2D)obj;
		if (frame == null || val3 == null)
		{
			return val;
		}
		float num = val.X + (float)val3.GetWidth() * val2.X * 0.5f;
		float num2 = val.Y + (float)val3.GetHeight() * val2.Y * 0.5f;
		return new Vector2(num - (float)frame.GetWidth() * val2.X * 0.5f, num2 - (float)frame.GetHeight() * val2.Y * 0.5f);
	}

	private static AttackFrame FindFrameAtOrBefore(IReadOnlyList<AttackFrame> frames, double time)
	{
		AttackFrame result = frames[0];
		foreach (AttackFrame frame in frames)
		{
			if (frame.Time > time)
			{
				break;
			}
			result = frame;
		}
		return result;
	}

	private void ConnectAnimationFinished()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		if (_animationPlayer != null && !_animationFinishedConnected)
		{
			((AnimationMixer)_animationPlayer).AnimationFinished += new AnimationFinishedEventHandler(OnAnimationFinished);
			_animationFinishedConnected = true;
		}
	}

	private void OnAnimationFinished(StringName animationName)
	{
		if (_animationPlayer != null && ((object)animationName).ToString().StartsWith("attack_", StringComparison.Ordinal) && ((AnimationMixer)_animationPlayer).HasAnimation(StringName.op_Implicit("idle")))
		{
			_animationPlayer.Play(StringName.op_Implicit("idle"), -1.0, 1f, false);
		}
	}

	private void PlayVoiceSfx(IReadOnlyList<string> relativePaths, float volumeMult)
	{
		if (relativePaths.Count != 0 && !IsVoicePlaying())
		{
			string text = PickNonRepeatingVoice(relativePaths);
			AudioStreamPlayer val = ValencinaLocalSfx.PlaySfx(text, 0f, volumeMult, 0f, 1f, (Node?)(object)this);
			if (val != null)
			{
				_activeVoicePlayer = val;
				_lastVoiceRelativePath = text;
			}
		}
	}

	private bool IsVoicePlaying()
	{
		if (_activeVoicePlayer != null && GodotObject.IsInstanceValid((GodotObject)(object)_activeVoicePlayer))
		{
			return _activeVoicePlayer.Playing;
		}
		return false;
	}

	private string PickNonRepeatingVoice(IReadOnlyList<string> relativePaths)
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
		while (text == _lastVoiceRelativePath);
		return text;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Expected O, but got Unknown
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Expected O, but got Unknown
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Expected O, but got Unknown
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Expected O, but got Unknown
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Expected O, but got Unknown
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_0373: Unknown result type (might be due to invalid IL or missing references)
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
		return new List<MethodInfo>(14)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.PlayAttack, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)2, StringName.op_Implicit("hitCount"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.PlayTurnStartVoice, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.EnsureAttackSprite, new PropertyInfo((Type)24, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Sprite2D"), false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.LoadAttackFrames, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.BuildAnimationLibrary, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.BuildIdleAnimation, new PropertyInfo((Type)24, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Animation"), false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.ResetToIdleVisualState, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.AddDiscreteTrack, new PropertyInfo((Type)2, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)33, new List<PropertyInfo>
			{
				new PropertyInfo((Type)24, StringName.op_Implicit("animation"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Animation"), false),
				new PropertyInfo((Type)4, StringName.op_Implicit("path"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.AttackTexture, new PropertyInfo((Type)24, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Texture2D"), false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)4, StringName.op_Implicit("name"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.GetRightAlignedPosition, new PropertyInfo((Type)5, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)24, StringName.op_Implicit("frame"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Texture2D"), false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.ConnectAnimationFinished, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.OnAnimationFinished, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)21, StringName.op_Implicit("animationName"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.IsVoicePlaying, new PropertyInfo((Type)1, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		if ((ref method) == MethodName._Ready && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			((Node)this)._Ready();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.PlayAttack && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			PlayAttack(VariantUtils.ConvertTo<int>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.PlayTurnStartVoice && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			PlayTurnStartVoice();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.EnsureAttackSprite && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			Sprite2D val = EnsureAttackSprite();
			ret = VariantUtils.CreateFrom<Sprite2D>(ref val);
			return true;
		}
		if ((ref method) == MethodName.LoadAttackFrames && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			LoadAttackFrames();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.BuildAnimationLibrary && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			BuildAnimationLibrary();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.BuildIdleAnimation && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			Animation val2 = BuildIdleAnimation();
			ret = VariantUtils.CreateFrom<Animation>(ref val2);
			return true;
		}
		if ((ref method) == MethodName.ResetToIdleVisualState && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			ResetToIdleVisualState();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.AddDiscreteTrack && ((NativeVariantPtrArgs)(ref args)).Count == 2)
		{
			int num = AddDiscreteTrack(VariantUtils.ConvertTo<Animation>(ref ((NativeVariantPtrArgs)(ref args))[0]), VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[1]));
			ret = VariantUtils.CreateFrom<int>(ref num);
			return true;
		}
		if ((ref method) == MethodName.AttackTexture && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			Texture2D val3 = AttackTexture(VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = VariantUtils.CreateFrom<Texture2D>(ref val3);
			return true;
		}
		if ((ref method) == MethodName.GetRightAlignedPosition && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			Vector2 rightAlignedPosition = GetRightAlignedPosition(VariantUtils.ConvertTo<Texture2D>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = VariantUtils.CreateFrom<Vector2>(ref rightAlignedPosition);
			return true;
		}
		if ((ref method) == MethodName.ConnectAnimationFinished && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			ConnectAnimationFinished();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.OnAnimationFinished && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			OnAnimationFinished(VariantUtils.ConvertTo<StringName>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.IsVoicePlaying && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			bool flag = IsVoicePlaying();
			ret = VariantUtils.CreateFrom<bool>(ref flag);
			return true;
		}
		return ((NCreatureVisuals)this).InvokeGodotClassMethod(ref method, args, ref ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if ((ref method) == MethodName.AddDiscreteTrack && ((NativeVariantPtrArgs)(ref args)).Count == 2)
		{
			int num = AddDiscreteTrack(VariantUtils.ConvertTo<Animation>(ref ((NativeVariantPtrArgs)(ref args))[0]), VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[1]));
			ret = VariantUtils.CreateFrom<int>(ref num);
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if ((ref method) == MethodName._Ready)
		{
			return true;
		}
		if ((ref method) == MethodName.PlayAttack)
		{
			return true;
		}
		if ((ref method) == MethodName.PlayTurnStartVoice)
		{
			return true;
		}
		if ((ref method) == MethodName.EnsureAttackSprite)
		{
			return true;
		}
		if ((ref method) == MethodName.LoadAttackFrames)
		{
			return true;
		}
		if ((ref method) == MethodName.BuildAnimationLibrary)
		{
			return true;
		}
		if ((ref method) == MethodName.BuildIdleAnimation)
		{
			return true;
		}
		if ((ref method) == MethodName.ResetToIdleVisualState)
		{
			return true;
		}
		if ((ref method) == MethodName.AddDiscreteTrack)
		{
			return true;
		}
		if ((ref method) == MethodName.AttackTexture)
		{
			return true;
		}
		if ((ref method) == MethodName.GetRightAlignedPosition)
		{
			return true;
		}
		if ((ref method) == MethodName.ConnectAnimationFinished)
		{
			return true;
		}
		if ((ref method) == MethodName.OnAnimationFinished)
		{
			return true;
		}
		if ((ref method) == MethodName.IsVoicePlaying)
		{
			return true;
		}
		return ((NCreatureVisuals)this).HasGodotClassMethod(ref method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if ((ref name) == PropertyName._idle)
		{
			_idle = VariantUtils.ConvertTo<Sprite2D>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._attack)
		{
			_attack = VariantUtils.ConvertTo<Sprite2D>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._animationPlayer)
		{
			_animationPlayer = VariantUtils.ConvertTo<AnimationPlayer>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._activeVoicePlayer)
		{
			_activeVoicePlayer = VariantUtils.ConvertTo<AudioStreamPlayer>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._lastVoiceRelativePath)
		{
			_lastVoiceRelativePath = VariantUtils.ConvertTo<string>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._animationFinishedConnected)
		{
			_animationFinishedConnected = VariantUtils.ConvertTo<bool>(ref value);
			return true;
		}
		return ((NCreatureVisuals)this).SetGodotClassPropertyValue(ref name, ref value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		if ((ref name) == PropertyName._idleFrames)
		{
			GodotObject[] idleFrames = (GodotObject[])(object)_idleFrames;
			value = VariantUtils.CreateFromSystemArrayOfGodotObject(idleFrames);
			return true;
		}
		if ((ref name) == PropertyName._idle)
		{
			value = VariantUtils.CreateFrom<Sprite2D>(ref _idle);
			return true;
		}
		if ((ref name) == PropertyName._attack)
		{
			value = VariantUtils.CreateFrom<Sprite2D>(ref _attack);
			return true;
		}
		if ((ref name) == PropertyName._animationPlayer)
		{
			value = VariantUtils.CreateFrom<AnimationPlayer>(ref _animationPlayer);
			return true;
		}
		if ((ref name) == PropertyName._activeVoicePlayer)
		{
			value = VariantUtils.CreateFrom<AudioStreamPlayer>(ref _activeVoicePlayer);
			return true;
		}
		if ((ref name) == PropertyName._lastVoiceRelativePath)
		{
			value = VariantUtils.CreateFrom<string>(ref _lastVoiceRelativePath);
			return true;
		}
		if ((ref name) == PropertyName._animationFinishedConnected)
		{
			value = VariantUtils.CreateFrom<bool>(ref _animationFinishedConnected);
			return true;
		}
		return ((NCreatureVisuals)this).GetGodotClassPropertyValue(ref name, ref value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		return new List<PropertyInfo>
		{
			new PropertyInfo((Type)28, PropertyName._idleFrames, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._idle, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._attack, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._animationPlayer, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._activeVoicePlayer, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)4, PropertyName._lastVoiceRelativePath, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)1, PropertyName._animationFinishedConnected, (PropertyHint)0, "", (PropertyUsageFlags)4096, false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		((NCreatureVisuals)this).SaveGodotObjectData(info);
		info.AddProperty(PropertyName._idle, Variant.From<Sprite2D>(ref _idle));
		info.AddProperty(PropertyName._attack, Variant.From<Sprite2D>(ref _attack));
		info.AddProperty(PropertyName._animationPlayer, Variant.From<AnimationPlayer>(ref _animationPlayer));
		info.AddProperty(PropertyName._activeVoicePlayer, Variant.From<AudioStreamPlayer>(ref _activeVoicePlayer));
		info.AddProperty(PropertyName._lastVoiceRelativePath, Variant.From<string>(ref _lastVoiceRelativePath));
		info.AddProperty(PropertyName._animationFinishedConnected, Variant.From<bool>(ref _animationFinishedConnected));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		((NCreatureVisuals)this).RestoreGodotObjectData(info);
		Variant val = default(Variant);
		if (info.TryGetProperty(PropertyName._idle, ref val))
		{
			_idle = ((Variant)(ref val)).As<Sprite2D>();
		}
		Variant val2 = default(Variant);
		if (info.TryGetProperty(PropertyName._attack, ref val2))
		{
			_attack = ((Variant)(ref val2)).As<Sprite2D>();
		}
		Variant val3 = default(Variant);
		if (info.TryGetProperty(PropertyName._animationPlayer, ref val3))
		{
			_animationPlayer = ((Variant)(ref val3)).As<AnimationPlayer>();
		}
		Variant val4 = default(Variant);
		if (info.TryGetProperty(PropertyName._activeVoicePlayer, ref val4))
		{
			_activeVoicePlayer = ((Variant)(ref val4)).As<AudioStreamPlayer>();
		}
		Variant val5 = default(Variant);
		if (info.TryGetProperty(PropertyName._lastVoiceRelativePath, ref val5))
		{
			_lastVoiceRelativePath = ((Variant)(ref val5)).As<string>();
		}
		Variant val6 = default(Variant);
		if (info.TryGetProperty(PropertyName._animationFinishedConnected, ref val6))
		{
			_animationFinishedConnected = ((Variant)(ref val6)).As<bool>();
		}
	}
}
