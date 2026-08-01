using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using Valencina.ValencinaCode.Settings;
using Valencina.ValencinaCode.Systems;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.UI;

public static class AmmoUiSync
{
	private const string LegacyCounterName = "AmmoCounter";

	private const string RootName = "ValencinaAmmoCounterUi";

	private const string CylinderName = "Cylinder";

	private const string AmmoTextName = "AmmoText";

	private const string SpentTextName = "SpentText";

	private const string DodgeTextName = "DodgeText";

	private const string TexturePath = "res://Valencina/images/ui/ammo/ammo_cylinder_ui.png";

	private static readonly Vector2 UiSize = new Vector2(139f, 139f);

	private static readonly Vector2 UiScale = new Vector2(1.3f, 1.3f);

	private static readonly Vector2 FallbackPosition = new Vector2(146.5f, 408.5f);

	private static readonly Vector2 EnergyCounterVisualCenter = new Vector2(48f, 88f);

	private static readonly Vector2 OffsetFromEnergyCounterCenter = new Vector2(18f, -224f);

	private static Control? _uiRoot;

	private static TextureRect? _cylinder;

	private static Label? _ammoText;

	private static Label? _spentText;

	private static Label? _dodgeText;

	private static Node? _combatRoom;

	private static Node? _energyAnchor;

	private static int? _lastAmmo;

	private static int? _lastMaxAmmo;

	private static bool _isSubscribedToScreenContext;

	public static void EnsureCombatUi(Node combatRoom)
	{
		if (ValencinaModConfig.DisableAmmoUi)
		{
			DestroyCombatUi();
			return;
		}
		_combatRoom = combatRoom;
		RemoveLegacyCounters(combatRoom);
		SubscribeToScreenContext();
		if (IsAlive((GodotObject?)(object)_uiRoot))
		{
			RepositionUi();
			RefreshAll(showFallbackLabel: false);
		}
		else
		{
			CreateUi(ResolveShakeSyncedParent(combatRoom, out _energyAnchor));
			RepositionUi();
			RefreshAll(showFallbackLabel: false);
		}
	}

	public static void DestroyCombatUi()
	{
		UnsubscribeFromScreenContext();
		if (IsAlive((GodotObject?)(object)_uiRoot))
		{
			((Node)_uiRoot).QueueFree();
		}
		_uiRoot = null;
		_cylinder = null;
		_ammoText = null;
		_spentText = null;
		_dodgeText = null;
		_combatRoom = null;
		_energyAnchor = null;
		_lastAmmo = null;
		_lastMaxAmmo = null;
	}

	public static void RefreshAll(bool showFallbackLabel)
	{
		if (ValencinaModConfig.DisableAmmoUi)
		{
			DestroyCombatUi();
			return;
		}
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val != null && val.Root != null)
		{
			if (!IsAlive((GodotObject?)(object)_uiRoot) && IsAlive((GodotObject?)(object)_combatRoom))
			{
				EnsureCombatUi(_combatRoom);
			}
			if (IsAlive((GodotObject?)(object)_uiRoot))
			{
				RepositionUi();
				RefreshAmmoControl(_uiRoot, showFallbackLabel);
			}
			RefreshLegacyLabels((Node)(object)val.Root);
		}
	}

	public static void RefreshAll()
	{
		RefreshAll(showFallbackLabel: true);
	}

	private static void CreateUi(Node parent)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		Control val = LoadUiScene() ?? CreateFallbackUi();
		((Node)val).Name = StringName.op_Implicit("ValencinaAmmoCounterUi");
		((CanvasItem)val).Visible = false;
		val.MouseFilter = (MouseFilterEnum)2;
		val.Scale = UiScale;
		if (val.Size == Vector2.Zero)
		{
			val.Size = UiSize;
		}
		if (val.CustomMinimumSize == Vector2.Zero)
		{
			val.CustomMinimumSize = val.Size;
		}
		((CanvasItem)val).ZIndex = -5;
		((CanvasItem)val).ZAsRelative = true;
		parent.AddChild((Node)(object)val, false, (InternalMode)0);
		CacheUiNodes(val);
		_lastAmmo = null;
		_lastMaxAmmo = null;
	}

	private static Control? LoadUiScene()
	{
		PackedScene val = null;
		string[] ammoUiSceneCandidates = MainFile.AmmoUiSceneCandidates;
		for (int i = 0; i < ammoUiSceneCandidates.Length; i++)
		{
			val = ResourceLoader.Load<PackedScene>(ammoUiSceneCandidates[i], (string)null, (CacheMode)1);
			if (val != null)
			{
				break;
			}
		}
		if (val == null)
		{
			return null;
		}
		Control val2 = val.Instantiate<Control>((GenEditState)0);
		if (val2 == null)
		{
			return null;
		}
		return val2;
	}

	private static void CacheUiNodes(Control root)
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		_uiRoot = root;
		_cylinder = ((Node)root).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("Cylinder"));
		_ammoText = ((Node)root).GetNodeOrNull<Label>(NodePath.op_Implicit("AmmoText"));
		_spentText = ((Node)root).GetNodeOrNull<Label>(NodePath.op_Implicit("SpentText"));
		_dodgeText = ((Node)root).GetNodeOrNull<Label>(NodePath.op_Implicit("DodgeText"));
		if (IsAlive((GodotObject?)(object)_dodgeText))
		{
			((CanvasItem)_dodgeText).Visible = false;
		}
		IgnoreMouseRecursive((Node)(object)root);
		if (IsAlive((GodotObject?)(object)_cylinder))
		{
			TextureRect cylinder = _cylinder;
			if (cylinder.Texture == null)
			{
				Texture2D val = (cylinder.Texture = ResourceLoader.Load<Texture2D>("res://Valencina/images/ui/ammo/ammo_cylinder_ui.png", (string)null, (CacheMode)1));
			}
			((Control)_cylinder).PivotOffset = ((Control)_cylinder).Size / 2f;
		}
	}

	private static void IgnoreMouseRecursive(Node node)
	{
		Control val = (Control)(object)((node is Control) ? node : null);
		if (val != null)
		{
			val.MouseFilter = (MouseFilterEnum)2;
		}
		foreach (Node child in node.GetChildren(false))
		{
			IgnoreMouseRecursive(child);
		}
	}

	private static Control CreateFallbackUi()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Expected O, but got Unknown
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Expected O, but got Unknown
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Expected O, but got Unknown
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Expected O, but got Unknown
		Texture2D texture = ResourceLoader.Load<Texture2D>("res://Valencina/images/ui/ammo/ammo_cylinder_ui.png", (string)null, (CacheMode)1);
		Control val = new Control
		{
			Name = StringName.op_Implicit("ValencinaAmmoCounterUi"),
			Visible = false,
			MouseFilter = (MouseFilterEnum)2,
			Size = UiSize,
			CustomMinimumSize = UiSize,
			ZIndex = -5,
			ZAsRelative = true
		};
		TextureRect val2 = new TextureRect
		{
			Name = StringName.op_Implicit("Cylinder"),
			MouseFilter = (MouseFilterEnum)2,
			Size = UiSize,
			PivotOffset = UiSize / 2f,
			Texture = texture,
			Modulate = new Color(0.6f, 0.62f, 0.66f, 0.88f)
		};
		Label val3 = new Label
		{
			Name = StringName.op_Implicit("AmmoText"),
			MouseFilter = (MouseFilterEnum)2,
			Position = new Vector2(0f, 49.5f),
			Size = new Vector2(UiSize.X, 22f),
			Text = "6/6",
			HorizontalAlignment = (HorizontalAlignment)1,
			VerticalAlignment = (VerticalAlignment)1
		};
		((Control)val3).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 18);
		((Control)val3).AddThemeColorOverride(StringName.op_Implicit("font_color"), new Color(1f, 0.88f, 0.48f, 1f));
		((Control)val3).AddThemeColorOverride(StringName.op_Implicit("font_outline_color"), new Color(0.09f, 0.055f, 0.025f, 1f));
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("outline_size"), 4);
		Label val4 = new Label
		{
			Name = StringName.op_Implicit("SpentText"),
			MouseFilter = (MouseFilterEnum)2,
			Position = new Vector2(0f, 69.5f),
			Size = new Vector2(UiSize.X, 22f),
			Text = "-0",
			Visible = false,
			HorizontalAlignment = (HorizontalAlignment)1,
			VerticalAlignment = (VerticalAlignment)1
		};
		((Control)val4).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 16);
		((Control)val4).AddThemeColorOverride(StringName.op_Implicit("font_color"), new Color(1f, 0.58f, 0.22f, 1f));
		((Control)val4).AddThemeColorOverride(StringName.op_Implicit("font_outline_color"), new Color(0.09f, 0.035f, 0.02f, 1f));
		((Control)val4).AddThemeConstantOverride(StringName.op_Implicit("outline_size"), 4);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val4, false, (InternalMode)0);
		return val;
	}

	private static Label CreateDodgeText(Control root)
	{
		Label val = BuildDodgeTextLabel();
		((Node)root).AddChild((Node)(object)val, false, (InternalMode)0);
		return val;
	}

	private static Label BuildDodgeTextLabel()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Expected O, but got Unknown
		Label val = new Label
		{
			Name = StringName.op_Implicit("DodgeText"),
			MouseFilter = (MouseFilterEnum)2,
			Position = new Vector2(0f, -23f),
			Size = new Vector2(UiSize.X, 26f),
			Text = "0",
			HorizontalAlignment = (HorizontalAlignment)1,
			VerticalAlignment = (VerticalAlignment)1
		};
		((Control)val).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 24);
		((Control)val).AddThemeColorOverride(StringName.op_Implicit("font_color"), new Color(0.74f, 0.94f, 1f, 1f));
		((Control)val).AddThemeColorOverride(StringName.op_Implicit("font_outline_color"), new Color(0.02f, 0.04f, 0.07f, 1f));
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("outline_size"), 5);
		return val;
	}

	private static void SubscribeToScreenContext()
	{
		if (!_isSubscribedToScreenContext)
		{
			ActiveScreenContext.Instance.Updated += OnActiveScreenContextUpdated;
			_isSubscribedToScreenContext = true;
		}
	}

	private static void UnsubscribeFromScreenContext()
	{
		if (_isSubscribedToScreenContext)
		{
			ActiveScreenContext.Instance.Updated -= OnActiveScreenContextUpdated;
			_isSubscribedToScreenContext = false;
		}
	}

	private static void OnActiveScreenContextUpdated()
	{
		RefreshAll(showFallbackLabel: false);
	}

	private static void RefreshAmmoControl(Control root, bool showFallbackLabel)
	{
		int num = AmmoSystem.CurrentAmmo();
		int num2 = AmmoSystem.MaxAmmoFor();
		int num3 = AmmoSystem.AmmoSpentThisTurn();
		((CanvasItem)root).Visible = ShouldShowCombatUi(showFallbackLabel);
		if (IsAlive((GodotObject?)(object)_ammoText))
		{
			_ammoText.Text = $"{num}/{num2}";
		}
		if (IsAlive((GodotObject?)(object)_spentText))
		{
			_spentText.Text = ((num3 > 0) ? $"-{num3}" : "-0");
			((CanvasItem)_spentText).Visible = num3 > 0;
		}
		RefreshDodgeText();
		if (IsAlive((GodotObject?)(object)_cylinder))
		{
			TintCylinder(_cylinder, num3);
			AnimateCylinderIfAmmoChanged(_cylinder, num, num2);
		}
	}

	private static void RefreshDodgeText()
	{
		if (IsAlive((GodotObject?)(object)_dodgeText))
		{
			((CanvasItem)_dodgeText).Visible = false;
		}
	}

	private static void TintCylinder(TextureRect cylinder, int spentThisTurn)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp((float)spentThisTurn / 6f, 0f, 1f);
		float num2 = 0.6f + 0.39999998f * num;
		float num3 = 0.62f + 0.19999999f * num;
		float num4 = 0.66f + -0.28000003f * num;
		((CanvasItem)cylinder).Modulate = new Color(num2, num3, num4, 0.88f);
	}

	private static void AnimateCylinderIfAmmoChanged(TextureRect cylinder, int ammo, int maxAmmo)
	{
		if (_lastAmmo.HasValue && _lastMaxAmmo.HasValue && _lastMaxAmmo.Value == maxAmmo)
		{
			int num = ammo - _lastAmmo.Value;
			if (num < 0)
			{
				int num2 = -num;
				float duration = Mathf.Min(0.34f, 0.12f + 0.055f * (float)num2);
				RotateCylinder(cylinder, -60f * (float)num2, duration);
				ValencinaLocalSfx.PlayCylinderTicks((Node)(object)cylinder, num2);
			}
			else if (num > 0)
			{
				RotateCylinder(cylinder, 360f, 0.52f);
			}
		}
		_lastAmmo = ammo;
		_lastMaxAmmo = maxAmmo;
	}

	private static void RotateCylinder(TextureRect cylinder, float degrees, float duration)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		float num = degrees * ((float)Math.PI / 180f);
		((Node)cylinder).CreateTween().TweenProperty((GodotObject)(object)cylinder, NodePath.op_Implicit("rotation"), Variant.op_Implicit(((Control)cylinder).Rotation + num), (double)duration);
	}

	private static Node ResolveShakeSyncedParent(Node combatRoom, out Node? energyAnchor)
	{
		NCombatRoom val = (NCombatRoom)(object)((combatRoom is NCombatRoom) ? combatRoom : null);
		if (val != null)
		{
			NCombatUi ui = val.Ui;
			energyAnchor = (Node?)(object)((ui != null) ? ui.EnergyCounterContainer : null);
			Node? obj = energyAnchor;
			Node val2 = ((obj != null) ? obj.GetParent() : null);
			if (val2 != null)
			{
				return val2;
			}
		}
		energyAnchor = FindEnergyCounterLikeNode(combatRoom);
		Node? obj2 = energyAnchor;
		Node val3 = ((obj2 != null) ? obj2.GetParent() : null);
		if (val3 != null)
		{
			return val3;
		}
		return FindFirstByNameOrType(combatRoom, "Hud", "HUD", "CombatUi", "CombatUI", "Energy") ?? combatRoom;
	}

	private static Node? FindEnergyCounterLikeNode(Node node)
	{
		Node val = FindFirstByNameOrType(node, "NEnergyCounter", "EnergyCounter");
		if (val != null)
		{
			return val;
		}
		return FindFirstByNameOrType(node, "EnergyOrb", "Energy");
	}

	private static Node? FindFirstByNameOrType(Node node, params string[] needles)
	{
		string text = ((object)node.Name).ToString();
		string name = ((object)node).GetType().Name;
		foreach (string value in needles)
		{
			if (text.Contains(value, StringComparison.OrdinalIgnoreCase) || name.Contains(value, StringComparison.OrdinalIgnoreCase))
			{
				return node;
			}
		}
		foreach (Node child in node.GetChildren(false))
		{
			Node val = FindFirstByNameOrType(child, needles);
			if (val != null)
			{
				return val;
			}
		}
		return null;
	}

	private static void RepositionUi()
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (IsAlive((GodotObject?)(object)_uiRoot))
		{
			if (IsAlive((GodotObject?)(object)_energyAnchor))
			{
				Vector2 energyAnchorCenter = GetEnergyAnchorCenter(_energyAnchor);
				Vector2 val = ((_uiRoot.Size == Vector2.Zero) ? UiSize : _uiRoot.Size) * _uiRoot.Scale;
				SetGlobalPosition(_uiRoot, energyAnchorCenter + OffsetFromEnergyCounterCenter - val / 2f);
			}
			else
			{
				_uiRoot.Position = FallbackPosition;
			}
		}
	}

	private static Vector2 GetEnergyAnchorCenter(Node node)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (IsCombatEnergyCounterContainer(node))
		{
			return GetGlobalPosition(node) + EnergyCounterVisualCenter;
		}
		return GetGlobalCenter(node);
	}

	private static bool IsCombatEnergyCounterContainer(Node node)
	{
		return string.Equals(((object)node.Name).ToString(), "EnergyCounterContainer", StringComparison.Ordinal);
	}

	private static Vector2 GetGlobalCenter(Node node)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		Control val = (Control)(object)((node is Control) ? node : null);
		if (val == null)
		{
			Node2D val2 = (Node2D)(object)((node is Node2D) ? node : null);
			if (val2 != null)
			{
				return val2.GlobalPosition;
			}
			return Vector2.Zero;
		}
		return val.GlobalPosition + val.Size / 2f;
	}

	private static Vector2 GetGlobalPosition(Node node)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Control val = (Control)(object)((node is Control) ? node : null);
		if (val == null)
		{
			Node2D val2 = (Node2D)(object)((node is Node2D) ? node : null);
			if (val2 != null)
			{
				return val2.GlobalPosition;
			}
			return Vector2.Zero;
		}
		return val.GlobalPosition;
	}

	private static void SetGlobalPosition(Control control, Vector2 globalPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		control.GlobalPosition = globalPosition;
	}

	private static void RemoveLegacyCounters(Node root)
	{
		List<Node> list = new List<Node>();
		CollectLegacyCounters(root, list);
		foreach (Node item in list)
		{
			item.QueueFree();
		}
	}

	private static void CollectLegacyCounters(Node node, List<Node> toRemove)
	{
		if (((object)node.Name).ToString() == "AmmoCounter")
		{
			toRemove.Add(node);
		}
		foreach (Node child in node.GetChildren(false))
		{
			CollectLegacyCounters(child, toRemove);
		}
	}

	private static void RefreshLegacyLabels(Node node)
	{
		if (((object)node.Name).ToString() == "AmmoCounter")
		{
			Label val = (Label)(object)((node is Label) ? node : null);
			if (val != null)
			{
				val.Text = AmmoSystem.DisplayText();
				((CanvasItem)val).Visible = false;
			}
			else
			{
				Control val2 = (Control)(object)((node is Control) ? node : null);
				if (val2 != null)
				{
					((CanvasItem)val2).Visible = false;
				}
			}
		}
		foreach (Node child in node.GetChildren(false))
		{
			RefreshLegacyLabels(child);
		}
	}

	private static bool IsAlive(GodotObject? obj)
	{
		if (obj != null)
		{
			return GodotObject.IsInstanceValid(obj);
		}
		return false;
	}

	private static bool ShouldShowCombatUi(bool showFallbackLabel)
	{
		if (ValencinaModConfig.DisableAmmoUi)
		{
			return false;
		}
		if (!showFallbackLabel && !AmmoSystem.HasFrontPower)
		{
			return false;
		}
		Node? combatRoom = _combatRoom;
		NCombatRoom val = (NCombatRoom)(object)((combatRoom is NCombatRoom) ? combatRoom : null);
		if (val == null || !IsAlive((GodotObject?)(object)val))
		{
			return false;
		}
		if (!ActiveScreenContext.Instance.IsCurrent((IScreenContext)(object)val))
		{
			return false;
		}
		NCombatUi ui = val.Ui;
		if (ui == null)
		{
			return true;
		}
		NPlayerHand hand = ui.Hand;
		return ((hand != null) ? new bool?(hand.IsInCardSelection) : ((bool?)null)) != true;
	}
}
