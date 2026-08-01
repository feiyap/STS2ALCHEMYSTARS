using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Valencina.ValencinaCode.Settings;

namespace Valencina.ValencinaCode.Vfx;

[ScriptPath("res://ValencinaCode/Vfx/ShinAuraSceneNode.cs")]
public class ShinAuraSceneNode : Node2D
{
	public class MethodName : MethodName
	{
		public static readonly StringName _Ready = StringName.op_Implicit("_Ready");

		public static readonly StringName MoveToBackOfVisuals = StringName.op_Implicit("MoveToBackOfVisuals");

		public static readonly StringName SetAuraActive = StringName.op_Implicit("SetAuraActive");

		public static readonly StringName _Process = StringName.op_Implicit("_Process");

		public static readonly StringName ApplyFrame = StringName.op_Implicit("ApplyFrame");

		public static readonly StringName ConfigureShinLayer = StringName.op_Implicit("ConfigureShinLayer");

		public static readonly StringName IsOwnerCurrentlyShinPowered = StringName.op_Implicit("IsOwnerCurrentlyShinPowered");

		public static readonly StringName FindOwnerCreatureNode = StringName.op_Implicit("FindOwnerCreatureNode");

		public static readonly StringName GetBodyDeltaForCurrentAnimation = StringName.op_Implicit("GetBodyDeltaForCurrentAnimation");

		public static readonly StringName GetCurrentBodySprite = StringName.op_Implicit("GetCurrentBodySprite");

		public static readonly StringName GetTopLeftAlignedDelta = StringName.op_Implicit("GetTopLeftAlignedDelta");

		public static readonly StringName GetSpriteTopLeft = StringName.op_Implicit("GetSpriteTopLeft");

		public static readonly StringName IsDisposalBodySprite = StringName.op_Implicit("IsDisposalBodySprite");

		public static readonly StringName GetCachedOrFind = StringName.op_Implicit("GetCachedOrFind");
	}

	public class PropertyName : PropertyName
	{
		public static readonly StringName _shinLayer = StringName.op_Implicit("_shinLayer");

		public static readonly StringName _shaderMaterial = StringName.op_Implicit("_shaderMaterial");

		public static readonly StringName _idle = StringName.op_Implicit("_idle");

		public static readonly StringName _attackBody = StringName.op_Implicit("_attackBody");

		public static readonly StringName _blockHit = StringName.op_Implicit("_blockHit");

		public static readonly StringName _damageFrame = StringName.op_Implicit("_damageFrame");

		public static readonly StringName _deathMiss = StringName.op_Implicit("_deathMiss");

		public static readonly StringName _attack2Body = StringName.op_Implicit("_attack2Body");

		public static readonly StringName _disposalVfx = StringName.op_Implicit("_disposalVfx");

		public static readonly StringName _precognitionDodgeFrame = StringName.op_Implicit("_precognitionDodgeFrame");

		public static readonly StringName _baseAuraPosition = StringName.op_Implicit("_baseAuraPosition");

		public static readonly StringName _referenceBodyPosition = StringName.op_Implicit("_referenceBodyPosition");

		public static readonly StringName _trackedBodyDelta = StringName.op_Implicit("_trackedBodyDelta");

		public static readonly StringName _phase = StringName.op_Implicit("_phase");
	}

	public class SignalName : SignalName
	{
	}

	private const string ShaderPath = "res://Valencina/shaders/vfx/shin_effect.gdshader";

	private const string MainTexturePath = "res://Valencina/images/vfx/shin/shin.png";

	private const string Noise03Path = "res://Valencina/images/vfx/shin/noise_03.png";

	private const string Noise04Path = "res://Valencina/images/vfx/shin/noise_04.png";

	private const string ThreadNoisePath = "res://Valencina/images/vfx/shin/thread_noise.png";

	private static readonly Vector2 FallbackAuraPosition = new Vector2(-28f, -230f);

	private static readonly Vector2 FallbackLayerPosition = new Vector2(20f, -90f);

	private static readonly Vector2 FallbackLayerScale = new Vector2(0.8023033f, 1.5431862f);

	private static readonly Color FallbackLayerColor = new Color(1f, 0.82f, 0.35f, 0.72f);

	private static readonly Vector2 SlowNoise03Speed = new Vector2(0.0125f, -0.15f);

	private static readonly Vector2 SlowNoise04Speed = new Vector2(-0.025f, -0.1f);

	private static readonly Vector2 SlowThreadSpeed = new Vector2(0f, 0.125f);

	private const float SlowPulseSpeed = 0.27f;

	private Sprite2D? _shinLayer;

	private ShaderMaterial? _shaderMaterial;

	private Sprite2D? _idle;

	private Sprite2D? _attackBody;

	private Sprite2D? _blockHit;

	private Sprite2D? _damageFrame;

	private Sprite2D? _deathMiss;

	private Sprite2D? _attack2Body;

	private Sprite2D? _disposalVfx;

	private Sprite2D? _precognitionDodgeFrame;

	private Vector2 _baseAuraPosition = FallbackAuraPosition;

	private Vector2 _referenceBodyPosition = new Vector2(-4f, -176f);

	private Vector2 _trackedBodyDelta = Vector2.Zero;

	private float _phase;

	public override void _Ready()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		_shinLayer = ((Node)this).GetNodeOrNull<Sprite2D>(NodePath.op_Implicit("ShinLayer"));
		if (_shinLayer == null)
		{
			_shinLayer = new Sprite2D
			{
				Name = StringName.op_Implicit("ShinLayer"),
				Position = FallbackLayerPosition,
				Scale = FallbackLayerScale,
				SelfModulate = FallbackLayerColor
			};
			((Node)this).AddChild((Node)(object)_shinLayer, false, (InternalMode)0);
		}
		Node parent = ((Node)this).GetParent();
		if (parent != null)
		{
			_idle = parent.GetNodeOrNull<Sprite2D>(NodePath.op_Implicit("Idle"));
			_attackBody = parent.GetNodeOrNull<Sprite2D>(NodePath.op_Implicit("AttackBody"));
			_blockHit = parent.GetNodeOrNull<Sprite2D>(NodePath.op_Implicit("BlockHit"));
			_damageFrame = parent.GetNodeOrNull<Sprite2D>(NodePath.op_Implicit("ValencinaDamageFrame"));
			_deathMiss = parent.GetNodeOrNull<Sprite2D>(NodePath.op_Implicit("DeathMiss"));
			_attack2Body = parent.GetNodeOrNull<Sprite2D>(NodePath.op_Implicit("Attack2/Body"));
			_disposalVfx = parent.GetNodeOrNull<Sprite2D>(NodePath.op_Implicit("DisposalVfx"));
			_precognitionDodgeFrame = parent.GetNodeOrNull<Sprite2D>(NodePath.op_Implicit("PrecognitionDodgeMissFrame"));
		}
		_baseAuraPosition = ((Node2D)this).Position;
		if (_baseAuraPosition == Vector2.Zero)
		{
			_baseAuraPosition = FallbackAuraPosition;
		}
		Sprite2D? idle = _idle;
		_referenceBodyPosition = (Vector2)((idle != null) ? ((Node2D)idle).Position : new Vector2(-4f, -176f));
		_trackedBodyDelta = Vector2.Zero;
		((CanvasItem)this).ZIndex = 0;
		((CanvasItem)this).ZAsRelative = true;
		MoveToBackOfVisuals();
		ConfigureShinLayer();
		if (Engine.IsEditorHint())
		{
			((CanvasItem)this).Visible = true;
			((Node)this).SetProcess(true);
			ApplyFrame(0f);
		}
		else
		{
			SetAuraActive(IsOwnerCurrentlyShinPowered());
		}
	}

	private void MoveToBackOfVisuals()
	{
		Node parent = ((Node)this).GetParent();
		if (parent != null && parent.IsNodeReady())
		{
			parent.MoveChild((Node)(object)this, 0);
		}
	}

	public void SetAuraActive(bool active)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (active && ValencinaModConfig.DisableShinAuraEffect)
		{
			active = false;
		}
		((CanvasItem)this).Visible = active;
		((Node)this).SetProcess(active);
		if (_shinLayer != null)
		{
			((CanvasItem)_shinLayer).Visible = active;
		}
		if (active)
		{
			_phase = 0f;
			_trackedBodyDelta = GetBodyDeltaForCurrentAnimation();
			ApplyFrame(0f);
		}
	}

	public override void _Process(double delta)
	{
		if (((CanvasItem)this).Visible)
		{
			_phase += (float)delta;
			ApplyFrame(_phase);
		}
	}

	private void ApplyFrame(float phase)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		Vector2 bodyDeltaForCurrentAnimation = GetBodyDeltaForCurrentAnimation();
		float num = (Engine.IsEditorHint() ? 1f : 0.34f);
		_trackedBodyDelta = ((Vector2)(ref _trackedBodyDelta)).Lerp(bodyDeltaForCurrentAnimation, num);
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(0.35f * Mathf.Sin(phase * 0.1f), 0.3f * Mathf.Sin(phase * 0.08f + 0.5f));
		((Node2D)this).Position = _baseAuraPosition + _trackedBodyDelta + val;
	}

	private void ConfigureShinLayer()
	{
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Expected O, but got Unknown
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		if (_shinLayer == null)
		{
			return;
		}
		Texture2D val = ResourceLoader.Load<Texture2D>("res://Valencina/images/vfx/shin/shin.png", (string)null, (CacheMode)1);
		Shader val2 = ResourceLoader.Load<Shader>("res://Valencina/shaders/vfx/shin_effect.gdshader", (string)null, (CacheMode)1);
		Texture2D val3 = ResourceLoader.Load<Texture2D>("res://Valencina/images/vfx/shin/noise_03.png", (string)null, (CacheMode)1);
		Texture2D val4 = ResourceLoader.Load<Texture2D>("res://Valencina/images/vfx/shin/noise_04.png", (string)null, (CacheMode)1);
		Texture2D val5 = ResourceLoader.Load<Texture2D>("res://Valencina/images/vfx/shin/thread_noise.png", (string)null, (CacheMode)1);
		if (val == null)
		{
			GD.PushWarning("[ShinAura] Missing Shin texture: res://Valencina/images/vfx/shin/shin.png");
		}
		if (val2 == null)
		{
			GD.PushWarning("[ShinAura] Missing Shin shader: res://Valencina/shaders/vfx/shin_effect.gdshader");
		}
		_shinLayer.Centered = true;
		((Node2D)_shinLayer).Rotation = 0f;
		_shinLayer.FlipH = false;
		((CanvasItem)_shinLayer).ZIndex = 0;
		((CanvasItem)_shinLayer).ZAsRelative = true;
		if (_shinLayer.Texture == null && val != null)
		{
			_shinLayer.Texture = val;
		}
		if (((Node2D)_shinLayer).Position == Vector2.Zero)
		{
			((Node2D)_shinLayer).Position = FallbackLayerPosition;
		}
		if (((Node2D)_shinLayer).Scale == Vector2.One)
		{
			((Node2D)_shinLayer).Scale = FallbackLayerScale;
		}
		if (((CanvasItem)_shinLayer).SelfModulate == Colors.White)
		{
			((CanvasItem)_shinLayer).SelfModulate = FallbackLayerColor;
		}
		Material material = ((CanvasItem)_shinLayer).Material;
		ShaderMaterial val6 = (ShaderMaterial)(object)((material is ShaderMaterial) ? material : null);
		if (val6 != null)
		{
			ref ShaderMaterial? shaderMaterial = ref _shaderMaterial;
			Resource obj = ((Resource)val6).Duplicate(false);
			shaderMaterial = (ShaderMaterial?)(((object)((obj is ShaderMaterial) ? obj : null)) ?? ((object)val6));
			((CanvasItem)_shinLayer).Material = (Material)(object)_shaderMaterial;
			if (_shaderMaterial.Shader == null && val2 != null)
			{
				_shaderMaterial.Shader = val2;
			}
		}
		else if (val2 != null)
		{
			_shaderMaterial = new ShaderMaterial
			{
				Shader = val2
			};
			((CanvasItem)_shinLayer).Material = (Material)(object)_shaderMaterial;
		}
		if (_shaderMaterial != null)
		{
			if (val != null)
			{
				_shaderMaterial.SetShaderParameter(StringName.op_Implicit("main_tex"), Variant.op_Implicit((GodotObject)(object)val));
			}
			if (val3 != null)
			{
				_shaderMaterial.SetShaderParameter(StringName.op_Implicit("noise_03"), Variant.op_Implicit((GodotObject)(object)val3));
			}
			if (val4 != null)
			{
				_shaderMaterial.SetShaderParameter(StringName.op_Implicit("noise_04"), Variant.op_Implicit((GodotObject)(object)val4));
			}
			if (val5 != null)
			{
				_shaderMaterial.SetShaderParameter(StringName.op_Implicit("thread_noise_tex"), Variant.op_Implicit((GodotObject)(object)val5));
			}
			_shaderMaterial.SetShaderParameter(StringName.op_Implicit("noise_03_speed"), Variant.op_Implicit(SlowNoise03Speed));
			_shaderMaterial.SetShaderParameter(StringName.op_Implicit("noise_04_speed"), Variant.op_Implicit(SlowNoise04Speed));
			_shaderMaterial.SetShaderParameter(StringName.op_Implicit("thread_speed"), Variant.op_Implicit(SlowThreadSpeed));
			_shaderMaterial.SetShaderParameter(StringName.op_Implicit("pulse_speed"), Variant.op_Implicit(0.27f));
		}
	}

	private bool IsOwnerCurrentlyShinPowered()
	{
		return ShinAuraController.HasShinAuraPower(FindOwnerCreatureNode());
	}

	private NCreature? FindOwnerCreatureNode()
	{
		for (Node val = (Node)(object)this; val != null; val = val.GetParent())
		{
			NCreature val2 = (NCreature)(object)((val is NCreature) ? val : null);
			if (val2 != null)
			{
				return val2;
			}
		}
		return null;
	}

	private Vector2 GetBodyDeltaForCurrentAnimation()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		Sprite2D currentBodySprite = GetCurrentBodySprite();
		if (currentBodySprite == null)
		{
			return Vector2.Zero;
		}
		Vector2 val = (IsDisposalBodySprite(currentBodySprite) ? GetTopLeftAlignedDelta(currentBodySprite) : (((Node2D)currentBodySprite).Position - _referenceBodyPosition));
		val.X = Mathf.Clamp(val.X, -140f, 140f);
		val.Y = Mathf.Clamp(val.Y, -95f, 95f);
		return val;
	}

	private Sprite2D? GetCurrentBodySprite()
	{
		if (_attackBody != null && ((CanvasItem)_attackBody).Visible)
		{
			return _attackBody;
		}
		_attack2Body = GetCachedOrFind(_attack2Body, "Attack2/Body");
		if (_attack2Body != null && ((CanvasItem)_attack2Body).Visible)
		{
			return _attack2Body;
		}
		if (_blockHit != null && ((CanvasItem)_blockHit).Visible)
		{
			return _blockHit;
		}
		_disposalVfx = GetCachedOrFind(_disposalVfx, "DisposalVfx");
		if (_disposalVfx != null && ((CanvasItem)_disposalVfx).Visible)
		{
			return _disposalVfx;
		}
		_precognitionDodgeFrame = GetCachedOrFind(_precognitionDodgeFrame, "PrecognitionDodgeMissFrame");
		if (_precognitionDodgeFrame != null && ((CanvasItem)_precognitionDodgeFrame).Visible)
		{
			return _precognitionDodgeFrame;
		}
		_damageFrame = GetCachedOrFind(_damageFrame, "ValencinaDamageFrame");
		if (_damageFrame != null && ((CanvasItem)_damageFrame).Visible)
		{
			return _damageFrame;
		}
		if (_deathMiss != null && ((CanvasItem)_deathMiss).Visible)
		{
			return _deathMiss;
		}
		if (_idle != null)
		{
			return _idle;
		}
		return null;
	}

	private Vector2 GetTopLeftAlignedDelta(Sprite2D current)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		Sprite2D? idle = _idle;
		if (((idle != null) ? idle.Texture : null) == null || current.Texture == null)
		{
			return ((Node2D)current).Position - _referenceBodyPosition;
		}
		Vector2 spriteTopLeft = GetSpriteTopLeft(_idle);
		return GetSpriteTopLeft(current) - spriteTopLeft;
	}

	private static Vector2 GetSpriteTopLeft(Sprite2D sprite)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (sprite.Texture == null)
		{
			return ((Node2D)sprite).Position;
		}
		Vector2 val = sprite.Texture.GetSize() * new Vector2(Mathf.Abs(((Node2D)sprite).Scale.X), Mathf.Abs(((Node2D)sprite).Scale.Y)) * 0.5f;
		return ((Node2D)sprite).Position - val;
	}

	private bool IsDisposalBodySprite(Sprite2D sprite)
	{
		_disposalVfx = GetCachedOrFind(_disposalVfx, "DisposalVfx");
		if (_disposalVfx != null)
		{
			return sprite == _disposalVfx;
		}
		return false;
	}

	private Sprite2D? GetCachedOrFind(Sprite2D? cached, string path)
	{
		if (cached != null && GodotObject.IsInstanceValid((GodotObject)(object)cached))
		{
			return cached;
		}
		Node parent = ((Node)this).GetParent();
		if (parent == null)
		{
			return null;
		}
		return parent.GetNodeOrNull<Sprite2D>(NodePath.op_Implicit(path));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Expected O, but got Unknown
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Expected O, but got Unknown
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Expected O, but got Unknown
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_0311: Expected O, but got Unknown
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Expected O, but got Unknown
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ac: Expected O, but got Unknown
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03da: Expected O, but got Unknown
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0401: Unknown result type (might be due to invalid IL or missing references)
		return new List<MethodInfo>(14)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.MoveToBackOfVisuals, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.SetAuraActive, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)1, StringName.op_Implicit("active"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName._Process, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)3, StringName.op_Implicit("delta"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.ApplyFrame, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)3, StringName.op_Implicit("phase"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.ConfigureShinLayer, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.IsOwnerCurrentlyShinPowered, new PropertyInfo((Type)1, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.FindOwnerCreatureNode, new PropertyInfo((Type)24, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Control"), false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.GetBodyDeltaForCurrentAnimation, new PropertyInfo((Type)5, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.GetCurrentBodySprite, new PropertyInfo((Type)24, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Sprite2D"), false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.GetTopLeftAlignedDelta, new PropertyInfo((Type)5, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)24, StringName.op_Implicit("current"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Sprite2D"), false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.GetSpriteTopLeft, new PropertyInfo((Type)5, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)33, new List<PropertyInfo>
			{
				new PropertyInfo((Type)24, StringName.op_Implicit("sprite"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Sprite2D"), false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.IsDisposalBodySprite, new PropertyInfo((Type)1, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)24, StringName.op_Implicit("sprite"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Sprite2D"), false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.GetCachedOrFind, new PropertyInfo((Type)24, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Sprite2D"), false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)24, StringName.op_Implicit("cached"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Sprite2D"), false),
				new PropertyInfo((Type)4, StringName.op_Implicit("path"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		if ((ref method) == MethodName._Ready && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			((Node)this)._Ready();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.MoveToBackOfVisuals && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			MoveToBackOfVisuals();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.SetAuraActive && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			SetAuraActive(VariantUtils.ConvertTo<bool>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName._Process && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			((Node)this)._Process(VariantUtils.ConvertTo<double>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.ApplyFrame && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			ApplyFrame(VariantUtils.ConvertTo<float>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.ConfigureShinLayer && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			ConfigureShinLayer();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.IsOwnerCurrentlyShinPowered && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			bool flag = IsOwnerCurrentlyShinPowered();
			ret = VariantUtils.CreateFrom<bool>(ref flag);
			return true;
		}
		if ((ref method) == MethodName.FindOwnerCreatureNode && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			NCreature val = FindOwnerCreatureNode();
			ret = VariantUtils.CreateFrom<NCreature>(ref val);
			return true;
		}
		if ((ref method) == MethodName.GetBodyDeltaForCurrentAnimation && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			Vector2 bodyDeltaForCurrentAnimation = GetBodyDeltaForCurrentAnimation();
			ret = VariantUtils.CreateFrom<Vector2>(ref bodyDeltaForCurrentAnimation);
			return true;
		}
		if ((ref method) == MethodName.GetCurrentBodySprite && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			Sprite2D currentBodySprite = GetCurrentBodySprite();
			ret = VariantUtils.CreateFrom<Sprite2D>(ref currentBodySprite);
			return true;
		}
		if ((ref method) == MethodName.GetTopLeftAlignedDelta && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			Vector2 topLeftAlignedDelta = GetTopLeftAlignedDelta(VariantUtils.ConvertTo<Sprite2D>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = VariantUtils.CreateFrom<Vector2>(ref topLeftAlignedDelta);
			return true;
		}
		if ((ref method) == MethodName.GetSpriteTopLeft && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			Vector2 spriteTopLeft = GetSpriteTopLeft(VariantUtils.ConvertTo<Sprite2D>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = VariantUtils.CreateFrom<Vector2>(ref spriteTopLeft);
			return true;
		}
		if ((ref method) == MethodName.IsDisposalBodySprite && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			bool flag2 = IsDisposalBodySprite(VariantUtils.ConvertTo<Sprite2D>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = VariantUtils.CreateFrom<bool>(ref flag2);
			return true;
		}
		if ((ref method) == MethodName.GetCachedOrFind && ((NativeVariantPtrArgs)(ref args)).Count == 2)
		{
			Sprite2D cachedOrFind = GetCachedOrFind(VariantUtils.ConvertTo<Sprite2D>(ref ((NativeVariantPtrArgs)(ref args))[0]), VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[1]));
			ret = VariantUtils.CreateFrom<Sprite2D>(ref cachedOrFind);
			return true;
		}
		return ((Node2D)this).InvokeGodotClassMethod(ref method, args, ref ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if ((ref method) == MethodName.GetSpriteTopLeft && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			Vector2 spriteTopLeft = GetSpriteTopLeft(VariantUtils.ConvertTo<Sprite2D>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = VariantUtils.CreateFrom<Vector2>(ref spriteTopLeft);
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
		if ((ref method) == MethodName.MoveToBackOfVisuals)
		{
			return true;
		}
		if ((ref method) == MethodName.SetAuraActive)
		{
			return true;
		}
		if ((ref method) == MethodName._Process)
		{
			return true;
		}
		if ((ref method) == MethodName.ApplyFrame)
		{
			return true;
		}
		if ((ref method) == MethodName.ConfigureShinLayer)
		{
			return true;
		}
		if ((ref method) == MethodName.IsOwnerCurrentlyShinPowered)
		{
			return true;
		}
		if ((ref method) == MethodName.FindOwnerCreatureNode)
		{
			return true;
		}
		if ((ref method) == MethodName.GetBodyDeltaForCurrentAnimation)
		{
			return true;
		}
		if ((ref method) == MethodName.GetCurrentBodySprite)
		{
			return true;
		}
		if ((ref method) == MethodName.GetTopLeftAlignedDelta)
		{
			return true;
		}
		if ((ref method) == MethodName.GetSpriteTopLeft)
		{
			return true;
		}
		if ((ref method) == MethodName.IsDisposalBodySprite)
		{
			return true;
		}
		if ((ref method) == MethodName.GetCachedOrFind)
		{
			return true;
		}
		return ((Node2D)this).HasGodotClassMethod(ref method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		if ((ref name) == PropertyName._shinLayer)
		{
			_shinLayer = VariantUtils.ConvertTo<Sprite2D>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._shaderMaterial)
		{
			_shaderMaterial = VariantUtils.ConvertTo<ShaderMaterial>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._idle)
		{
			_idle = VariantUtils.ConvertTo<Sprite2D>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._attackBody)
		{
			_attackBody = VariantUtils.ConvertTo<Sprite2D>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._blockHit)
		{
			_blockHit = VariantUtils.ConvertTo<Sprite2D>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._damageFrame)
		{
			_damageFrame = VariantUtils.ConvertTo<Sprite2D>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._deathMiss)
		{
			_deathMiss = VariantUtils.ConvertTo<Sprite2D>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._attack2Body)
		{
			_attack2Body = VariantUtils.ConvertTo<Sprite2D>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._disposalVfx)
		{
			_disposalVfx = VariantUtils.ConvertTo<Sprite2D>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._precognitionDodgeFrame)
		{
			_precognitionDodgeFrame = VariantUtils.ConvertTo<Sprite2D>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._baseAuraPosition)
		{
			_baseAuraPosition = VariantUtils.ConvertTo<Vector2>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._referenceBodyPosition)
		{
			_referenceBodyPosition = VariantUtils.ConvertTo<Vector2>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._trackedBodyDelta)
		{
			_trackedBodyDelta = VariantUtils.ConvertTo<Vector2>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._phase)
		{
			_phase = VariantUtils.ConvertTo<float>(ref value);
			return true;
		}
		return ((GodotObject)this).SetGodotClassPropertyValue(ref name, ref value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		if ((ref name) == PropertyName._shinLayer)
		{
			value = VariantUtils.CreateFrom<Sprite2D>(ref _shinLayer);
			return true;
		}
		if ((ref name) == PropertyName._shaderMaterial)
		{
			value = VariantUtils.CreateFrom<ShaderMaterial>(ref _shaderMaterial);
			return true;
		}
		if ((ref name) == PropertyName._idle)
		{
			value = VariantUtils.CreateFrom<Sprite2D>(ref _idle);
			return true;
		}
		if ((ref name) == PropertyName._attackBody)
		{
			value = VariantUtils.CreateFrom<Sprite2D>(ref _attackBody);
			return true;
		}
		if ((ref name) == PropertyName._blockHit)
		{
			value = VariantUtils.CreateFrom<Sprite2D>(ref _blockHit);
			return true;
		}
		if ((ref name) == PropertyName._damageFrame)
		{
			value = VariantUtils.CreateFrom<Sprite2D>(ref _damageFrame);
			return true;
		}
		if ((ref name) == PropertyName._deathMiss)
		{
			value = VariantUtils.CreateFrom<Sprite2D>(ref _deathMiss);
			return true;
		}
		if ((ref name) == PropertyName._attack2Body)
		{
			value = VariantUtils.CreateFrom<Sprite2D>(ref _attack2Body);
			return true;
		}
		if ((ref name) == PropertyName._disposalVfx)
		{
			value = VariantUtils.CreateFrom<Sprite2D>(ref _disposalVfx);
			return true;
		}
		if ((ref name) == PropertyName._precognitionDodgeFrame)
		{
			value = VariantUtils.CreateFrom<Sprite2D>(ref _precognitionDodgeFrame);
			return true;
		}
		if ((ref name) == PropertyName._baseAuraPosition)
		{
			value = VariantUtils.CreateFrom<Vector2>(ref _baseAuraPosition);
			return true;
		}
		if ((ref name) == PropertyName._referenceBodyPosition)
		{
			value = VariantUtils.CreateFrom<Vector2>(ref _referenceBodyPosition);
			return true;
		}
		if ((ref name) == PropertyName._trackedBodyDelta)
		{
			value = VariantUtils.CreateFrom<Vector2>(ref _trackedBodyDelta);
			return true;
		}
		if ((ref name) == PropertyName._phase)
		{
			value = VariantUtils.CreateFrom<float>(ref _phase);
			return true;
		}
		return ((GodotObject)this).GetGodotClassPropertyValue(ref name, ref value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		return new List<PropertyInfo>
		{
			new PropertyInfo((Type)24, PropertyName._shinLayer, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._shaderMaterial, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._idle, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._attackBody, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._blockHit, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._damageFrame, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._deathMiss, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._attack2Body, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._disposalVfx, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._precognitionDodgeFrame, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)5, PropertyName._baseAuraPosition, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)5, PropertyName._referenceBodyPosition, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)5, PropertyName._trackedBodyDelta, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)3, PropertyName._phase, (PropertyHint)0, "", (PropertyUsageFlags)4096, false)
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
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		((GodotObject)this).SaveGodotObjectData(info);
		info.AddProperty(PropertyName._shinLayer, Variant.From<Sprite2D>(ref _shinLayer));
		info.AddProperty(PropertyName._shaderMaterial, Variant.From<ShaderMaterial>(ref _shaderMaterial));
		info.AddProperty(PropertyName._idle, Variant.From<Sprite2D>(ref _idle));
		info.AddProperty(PropertyName._attackBody, Variant.From<Sprite2D>(ref _attackBody));
		info.AddProperty(PropertyName._blockHit, Variant.From<Sprite2D>(ref _blockHit));
		info.AddProperty(PropertyName._damageFrame, Variant.From<Sprite2D>(ref _damageFrame));
		info.AddProperty(PropertyName._deathMiss, Variant.From<Sprite2D>(ref _deathMiss));
		info.AddProperty(PropertyName._attack2Body, Variant.From<Sprite2D>(ref _attack2Body));
		info.AddProperty(PropertyName._disposalVfx, Variant.From<Sprite2D>(ref _disposalVfx));
		info.AddProperty(PropertyName._precognitionDodgeFrame, Variant.From<Sprite2D>(ref _precognitionDodgeFrame));
		info.AddProperty(PropertyName._baseAuraPosition, Variant.From<Vector2>(ref _baseAuraPosition));
		info.AddProperty(PropertyName._referenceBodyPosition, Variant.From<Vector2>(ref _referenceBodyPosition));
		info.AddProperty(PropertyName._trackedBodyDelta, Variant.From<Vector2>(ref _trackedBodyDelta));
		info.AddProperty(PropertyName._phase, Variant.From<float>(ref _phase));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		((GodotObject)this).RestoreGodotObjectData(info);
		Variant val = default(Variant);
		if (info.TryGetProperty(PropertyName._shinLayer, ref val))
		{
			_shinLayer = ((Variant)(ref val)).As<Sprite2D>();
		}
		Variant val2 = default(Variant);
		if (info.TryGetProperty(PropertyName._shaderMaterial, ref val2))
		{
			_shaderMaterial = ((Variant)(ref val2)).As<ShaderMaterial>();
		}
		Variant val3 = default(Variant);
		if (info.TryGetProperty(PropertyName._idle, ref val3))
		{
			_idle = ((Variant)(ref val3)).As<Sprite2D>();
		}
		Variant val4 = default(Variant);
		if (info.TryGetProperty(PropertyName._attackBody, ref val4))
		{
			_attackBody = ((Variant)(ref val4)).As<Sprite2D>();
		}
		Variant val5 = default(Variant);
		if (info.TryGetProperty(PropertyName._blockHit, ref val5))
		{
			_blockHit = ((Variant)(ref val5)).As<Sprite2D>();
		}
		Variant val6 = default(Variant);
		if (info.TryGetProperty(PropertyName._damageFrame, ref val6))
		{
			_damageFrame = ((Variant)(ref val6)).As<Sprite2D>();
		}
		Variant val7 = default(Variant);
		if (info.TryGetProperty(PropertyName._deathMiss, ref val7))
		{
			_deathMiss = ((Variant)(ref val7)).As<Sprite2D>();
		}
		Variant val8 = default(Variant);
		if (info.TryGetProperty(PropertyName._attack2Body, ref val8))
		{
			_attack2Body = ((Variant)(ref val8)).As<Sprite2D>();
		}
		Variant val9 = default(Variant);
		if (info.TryGetProperty(PropertyName._disposalVfx, ref val9))
		{
			_disposalVfx = ((Variant)(ref val9)).As<Sprite2D>();
		}
		Variant val10 = default(Variant);
		if (info.TryGetProperty(PropertyName._precognitionDodgeFrame, ref val10))
		{
			_precognitionDodgeFrame = ((Variant)(ref val10)).As<Sprite2D>();
		}
		Variant val11 = default(Variant);
		if (info.TryGetProperty(PropertyName._baseAuraPosition, ref val11))
		{
			_baseAuraPosition = ((Variant)(ref val11)).As<Vector2>();
		}
		Variant val12 = default(Variant);
		if (info.TryGetProperty(PropertyName._referenceBodyPosition, ref val12))
		{
			_referenceBodyPosition = ((Variant)(ref val12)).As<Vector2>();
		}
		Variant val13 = default(Variant);
		if (info.TryGetProperty(PropertyName._trackedBodyDelta, ref val13))
		{
			_trackedBodyDelta = ((Variant)(ref val13)).As<Vector2>();
		}
		Variant val14 = default(Variant);
		if (info.TryGetProperty(PropertyName._phase, ref val14))
		{
			_phase = ((Variant)(ref val14)).As<float>();
		}
	}
}
