using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.addons.mega_text;
using Valencina.ValencinaCode.Character;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Patches;

internal static class DodgeHealthBarOverlay
{
	private enum DisplayMode
	{
		None,
		Block,
		Dodge
	}

	private sealed class State
	{
		public int LastDodge;

		public int LastBlock;

		public bool CycleRunning;

		public bool PreferDodge = true;

		public DisplayMode LastMode;

		public bool IsFading;

		public int FadeSerial;

		public bool HasOriginalBlockLabelPosition;

		public Vector2 OriginalBlockLabelPosition;

		public readonly Dictionary<CanvasItem, bool> OriginalBlockGraphicVisibility = new Dictionary<CanvasItem, bool>();
	}

	private static readonly ConditionalWeakTable<NHealthBar, State> States = new ConditionalWeakTable<NHealthBar, State>();

	private static readonly FieldInfo? CreatureField = AccessTools.Field(typeof(NHealthBar), "_creature");

	private static readonly FieldInfo? BlockContainerField = AccessTools.Field(typeof(NHealthBar), "_blockContainer");

	private static readonly FieldInfo? BlockLabelField = AccessTools.Field(typeof(NHealthBar), "_blockLabel");

	private static readonly FieldInfo? BlockOutlineField = AccessTools.Field(typeof(NHealthBar), "_blockOutline");

	private static readonly FieldInfo? HpForegroundField = AccessTools.Field(typeof(NHealthBar), "_hpForeground");

	private static readonly FieldInfo? OriginalBlockPositionField = AccessTools.Field(typeof(NHealthBar), "_originalBlockPosition");

	private const string DodgeIconPath = "res://Valencina/images/ui/dodge_block_icon.png";

	private const string DodgeIconName = "ValencinaDodgeIcon";

	private static Texture2D? _dodgeIconTexture;

	private static readonly Color DodgeCyan = new Color("1AC6EF");

	private static readonly Color DodgeCyanDark = new Color("0B5A6C");

	private static readonly Color DodgeHpForeground = new Color("1AC6EF");

	private static readonly Color VanillaBlockOutline = new Color("1B3045");

	private static readonly Color VanillaHpForeground = new Color("F1373E");

	private static readonly Color VanillaBlockHpForeground = new Color("3B6FA3");

	private static readonly Color White = Colors.White;

	private const double CycleSeconds = 0.72;

	private const float DodgeLabelOffsetX = -3f;

	private const double FadeSeconds = 0.16;

	private const int FadeSteps = 4;

	public static Creature? GetCreatureForPatch(NHealthBar healthBar)
	{
		return GetCreature(healthBar);
	}

	public static bool IsSupportedCreature(Creature creature)
	{
		Player player = creature.Player;
		return ((player != null) ? player.Character : null) is Valencina.ValencinaCode.Character.Valencina;
	}

	public static void RefreshForCreature(Creature? creature)
	{
		if (creature == null || !IsSupportedCreature(creature))
		{
			return;
		}
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val == null || val.Root == null)
		{
			return;
		}
		foreach (NHealthBar item in EnumerateHealthBars((Node)(object)val.Root))
		{
			if (GetCreature(item) == creature)
			{
				Apply(item);
				return;
			}
		}
		NCombatRoom instance = NCombatRoom.Instance;
		NCreature val2 = ((instance != null) ? instance.GetCreatureNode(creature) : null);
		if (val2 == null)
		{
			return;
		}
		foreach (NHealthBar item2 in EnumerateHealthBars((Node)(object)val2))
		{
			if (GetCreature(item2) == creature)
			{
				Apply(item2);
				break;
			}
		}
	}

	public static void Apply(NHealthBar healthBar)
	{
		Creature creature = GetCreature(healthBar);
		if (creature == null || !IsSupportedCreature(creature))
		{
			return;
		}
		int num = creature.GetPower<InstantForesightPower>()?.DodgeValue ?? 0;
		int num2 = Math.Max(0, creature.Block);
		State orCreateValue = States.GetOrCreateValue(healthBar);
		orCreateValue.LastDodge = num;
		orCreateValue.LastBlock = num2;
		if (num2 > 0 && num > 0)
		{
			StartAlternatingCycle(healthBar, orCreateValue);
			DisplayMode lastMode = orCreateValue.LastMode;
			bool flag = (uint)(lastMode - 1) <= 1u;
			DisplayMode mode = (flag ? orCreateValue.LastMode : ((!orCreateValue.PreferDodge) ? DisplayMode.Block : DisplayMode.Dodge));
			if (!orCreateValue.IsFading)
			{
				ApplyMode(healthBar, creature, mode, num2, num);
			}
		}
		else
		{
			orCreateValue.PreferDodge = true;
			orCreateValue.FadeSerial++;
			orCreateValue.IsFading = false;
			if (num > 0)
			{
				ApplyMode(healthBar, creature, DisplayMode.Dodge, num2, num);
			}
			else
			{
				ApplyMode(healthBar, creature, (num2 > 0) ? DisplayMode.Block : DisplayMode.None, num2, num);
			}
		}
	}

	private static void StartAlternatingCycle(NHealthBar healthBar, State state)
	{
		if (!state.CycleRunning)
		{
			state.CycleRunning = true;
			RunAlternatingCycleAsync(healthBar, state);
		}
	}

	private static async Task RunAlternatingCycleAsync(NHealthBar healthBar, State state)
	{
		_ = 1;
		try
		{
			while (GodotObject.IsInstanceValid((GodotObject)(object)healthBar))
			{
				SceneTree tree = ((Node)healthBar).GetTree();
				if (tree != null)
				{
					SceneTreeTimer val = tree.CreateTimer(0.72, true, false, false);
					await ((GodotObject)healthBar).ToSignal((GodotObject)(object)val, SignalName.Timeout);
					Creature creature = GetCreature(healthBar);
					if (creature != null)
					{
						int num = creature.GetPower<InstantForesightPower>()?.DodgeValue ?? 0;
						int num2 = Math.Max(0, creature.Block);
						if (num > 0 && num2 > 0)
						{
							state.PreferDodge = !state.PreferDodge;
							state.LastDodge = num;
							state.LastBlock = num2;
							await FadeToModeAsync(healthBar, creature, state, (!state.PreferDodge) ? DisplayMode.Block : DisplayMode.Dodge, num2, num);
							continue;
						}
						break;
					}
					break;
				}
				break;
			}
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[DodgeHealthBar] alternate display stopped: " + ex.Message, 1);
		}
		finally
		{
			state.CycleRunning = false;
			state.IsFading = false;
			if (GodotObject.IsInstanceValid((GodotObject)(object)healthBar))
			{
				Apply(healthBar);
			}
		}
	}

	private static async Task FadeToModeAsync(NHealthBar healthBar, Creature creature, State state, DisplayMode mode, int realBlock, int dodge)
	{
		object? obj = BlockContainerField?.GetValue(healthBar);
		Control blockContainer = (Control)((obj is Control) ? obj : null);
		if (blockContainer == null)
		{
			ApplyMode(healthBar, creature, mode, realBlock, dodge);
			return;
		}
		int serial = ++state.FadeSerial;
		state.IsFading = true;
		try
		{
			await FadeContainerAlphaAsync(healthBar, blockContainer, 1f, 0.18f, serial, state);
			if (serial == state.FadeSerial && GodotObject.IsInstanceValid((GodotObject)(object)healthBar))
			{
				ApplyMode(healthBar, creature, mode, realBlock, dodge);
				SetContainerAlpha(blockContainer, 0.18f);
				await FadeContainerAlphaAsync(healthBar, blockContainer, 0.18f, 1f, serial, state);
			}
		}
		finally
		{
			if (serial == state.FadeSerial)
			{
				state.IsFading = false;
			}
			if (GodotObject.IsInstanceValid((GodotObject)(object)blockContainer))
			{
				SetContainerAlpha(blockContainer, 1f);
			}
		}
	}

	private static async Task FadeContainerAlphaAsync(NHealthBar healthBar, Control blockContainer, float from, float to, int serial, State state)
	{
		SceneTree tree = ((Node)healthBar).GetTree();
		if (tree == null)
		{
			SetContainerAlpha(blockContainer, to);
			return;
		}
		for (int i = 1; i <= 4; i++)
		{
			if (serial != state.FadeSerial || !GodotObject.IsInstanceValid((GodotObject)(object)healthBar) || !GodotObject.IsInstanceValid((GodotObject)(object)blockContainer))
			{
				return;
			}
			float num = (float)i / 4f;
			SetContainerAlpha(blockContainer, from + (to - from) * num);
			SceneTreeTimer val = tree.CreateTimer(0.04, true, false, false);
			await ((GodotObject)healthBar).ToSignal((GodotObject)(object)val, SignalName.Timeout);
		}
		if (serial == state.FadeSerial && GodotObject.IsInstanceValid((GodotObject)(object)blockContainer))
		{
			SetContainerAlpha(blockContainer, to);
		}
	}

	private static void SetContainerAlpha(Control blockContainer, float alpha)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Color modulate = ((CanvasItem)blockContainer).Modulate;
		modulate.A = alpha;
		((CanvasItem)blockContainer).Modulate = modulate;
	}

	private static void ApplyMode(NHealthBar healthBar, Creature creature, DisplayMode mode, int realBlock, int dodge)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		object? obj = BlockContainerField?.GetValue(healthBar);
		Control val = (Control)((obj is Control) ? obj : null);
		object? obj2 = BlockLabelField?.GetValue(healthBar);
		MegaLabel val2 = (MegaLabel)((obj2 is MegaLabel) ? obj2 : null);
		object? obj3 = BlockOutlineField?.GetValue(healthBar);
		Control val3 = (Control)((obj3 is Control) ? obj3 : null);
		object? obj4 = HpForegroundField?.GetValue(healthBar);
		Control val4 = (Control)((obj4 is Control) ? obj4 : null);
		State orCreateValue = States.GetOrCreateValue(healthBar);
		if (val == null || val2 == null)
		{
			return;
		}
		((CanvasItem)val).Modulate = White;
		if (OriginalBlockPositionField?.GetValue(healthBar) is Vector2 position)
		{
			val.Position = position;
		}
		switch (mode)
		{
		case DisplayMode.Dodge:
			((CanvasItem)val).Visible = true;
			if (val3 != null)
			{
				((CanvasItem)val3).Visible = true;
				((CanvasItem)val3).SelfModulate = DodgeCyanDark;
			}
			if (val4 != null)
			{
				((CanvasItem)val4).SelfModulate = DodgeHpForeground;
			}
			val2.SetTextAutoSize(dodge.ToString());
			ApplyBlockLabelPosition(val2, orCreateValue, dodgeMode: true);
			((CanvasItem)val2).SelfModulate = White;
			((CanvasItem)val2).Modulate = White;
			TryTintBlockGraphics((Node)(object)val, DodgeCyan, White);
			ApplyDodgeIcon(val, orCreateValue, visible: true);
			orCreateValue.LastMode = DisplayMode.Dodge;
			break;
		case DisplayMode.Block:
			if (realBlock <= 0)
			{
				ApplyMode(healthBar, creature, DisplayMode.None, realBlock, dodge);
				break;
			}
			((CanvasItem)val).Visible = true;
			if (val3 != null)
			{
				((CanvasItem)val3).Visible = true;
				((CanvasItem)val3).SelfModulate = VanillaBlockOutline;
			}
			if (val4 != null)
			{
				((CanvasItem)val4).SelfModulate = VanillaBlockHpForeground;
			}
			val2.SetTextAutoSize(realBlock.ToString());
			ApplyBlockLabelPosition(val2, orCreateValue, dodgeMode: false);
			((CanvasItem)val2).SelfModulate = White;
			((CanvasItem)val2).Modulate = White;
			ApplyDodgeIcon(val, orCreateValue, visible: false);
			ResetBlockGraphics((Node)(object)val);
			orCreateValue.LastMode = DisplayMode.Block;
			break;
		default:
			((CanvasItem)val).Visible = false;
			if (val3 != null)
			{
				((CanvasItem)val3).Visible = false;
				((CanvasItem)val3).SelfModulate = VanillaBlockOutline;
			}
			if (val4 != null)
			{
				((CanvasItem)val4).SelfModulate = VanillaHpForeground;
			}
			ApplyDodgeIcon(val, orCreateValue, visible: false);
			ApplyBlockLabelPosition(val2, orCreateValue, dodgeMode: false);
			ResetBlockGraphics((Node)(object)val);
			orCreateValue.LastMode = DisplayMode.None;
			break;
		}
	}

	private static void ApplyBlockLabelPosition(MegaLabel blockLabel, State state, bool dodgeMode)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (!state.HasOriginalBlockLabelPosition)
		{
			state.OriginalBlockLabelPosition = ((Control)blockLabel).Position;
			state.HasOriginalBlockLabelPosition = true;
		}
		((Control)blockLabel).Position = state.OriginalBlockLabelPosition + (Vector2)(dodgeMode ? new Vector2(-3f, 0f) : Vector2.Zero);
	}

	private static void ApplyDodgeIcon(Control blockContainer, State state, bool visible)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		TextureRect val = EnsureDodgeIcon(blockContainer);
		if (val == null)
		{
			if (!visible)
			{
				SetOriginalBlockGraphicsHidden((Node)(object)blockContainer, state, hidden: false);
			}
		}
		else
		{
			SetOriginalBlockGraphicsHidden((Node)(object)blockContainer, state, visible);
			((CanvasItem)val).Visible = visible;
			((CanvasItem)val).Modulate = White;
			((CanvasItem)val).SelfModulate = White;
		}
	}

	private static TextureRect? EnsureDodgeIcon(Control blockContainer)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		Node obj = ((Node)blockContainer).FindChild("ValencinaDodgeIcon", false, false);
		TextureRect val = (TextureRect)(object)((obj is TextureRect) ? obj : null);
		if (val != null)
		{
			return val;
		}
		Texture2D dodgeIconTexture = GetDodgeIconTexture();
		if (dodgeIconTexture == null)
		{
			return null;
		}
		TextureRect val2 = new TextureRect
		{
			Name = StringName.op_Implicit("ValencinaDodgeIcon"),
			Texture = dodgeIconTexture,
			Visible = false,
			MouseFilter = (MouseFilterEnum)2,
			ExpandMode = (ExpandModeEnum)1,
			StretchMode = (StretchModeEnum)5
		};
		((Control)val2).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		((Node)blockContainer).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)blockContainer).MoveChild((Node)(object)val2, 0);
		return val2;
	}

	private static Texture2D? GetDodgeIconTexture()
	{
		if (_dodgeIconTexture != null)
		{
			return _dodgeIconTexture;
		}
		_dodgeIconTexture = ResourceLoader.Load<Texture2D>("res://Valencina/images/ui/dodge_block_icon.png", string.Empty, (CacheMode)1);
		return _dodgeIconTexture;
	}

	private static void SetOriginalBlockGraphicsHidden(Node node, State state, bool hidden)
	{
		SetOriginalBlockGraphicsHiddenRecursive(node, state, hidden);
		if (!hidden)
		{
			state.OriginalBlockGraphicVisibility.Clear();
		}
	}

	private static void SetOriginalBlockGraphicsHiddenRecursive(Node node, State state, bool hidden)
	{
		foreach (Node child in node.GetChildren(false))
		{
			if (((object)child.Name).ToString() == "ValencinaDodgeIcon")
			{
				continue;
			}
			CanvasItem val = (CanvasItem)(object)((child is CanvasItem) ? child : null);
			if (val != null && !IsTextNode(child) && !ContainsTextNode(child))
			{
				bool value;
				if (hidden)
				{
					if (!state.OriginalBlockGraphicVisibility.ContainsKey(val))
					{
						state.OriginalBlockGraphicVisibility[val] = val.Visible;
					}
					val.Visible = false;
				}
				else if (state.OriginalBlockGraphicVisibility.TryGetValue(val, out value))
				{
					val.Visible = value;
				}
			}
			SetOriginalBlockGraphicsHiddenRecursive(child, state, hidden);
		}
	}

	private static bool ContainsTextNode(Node node)
	{
		foreach (Node child in node.GetChildren(false))
		{
			if (IsTextNode(child) || ContainsTextNode(child))
			{
				return true;
			}
		}
		return false;
	}

	private static Creature? GetCreature(NHealthBar healthBar)
	{
		object? obj = CreatureField?.GetValue(healthBar);
		return (Creature?)((obj is Creature) ? obj : null);
	}

	private static IEnumerable<NHealthBar> EnumerateHealthBars(Node root)
	{
		NHealthBar val = (NHealthBar)(object)((root is NHealthBar) ? root : null);
		if (val != null)
		{
			yield return val;
		}
		foreach (Node child in root.GetChildren(false))
		{
			foreach (NHealthBar item in EnumerateHealthBars(child))
			{
				yield return item;
			}
		}
	}

	private static void TryTintBlockGraphics(Node node, Color graphicColor, Color textColor)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		foreach (Node child in node.GetChildren(false))
		{
			if (IsTextNode(child))
			{
				CanvasItem val = (CanvasItem)(object)((child is CanvasItem) ? child : null);
				if (val != null)
				{
					val.SelfModulate = textColor;
				}
			}
			else
			{
				CanvasItem val2 = (CanvasItem)(object)((child is CanvasItem) ? child : null);
				if (val2 != null)
				{
					val2.SelfModulate = graphicColor;
				}
			}
			TryTintBlockGraphics(child, graphicColor, textColor);
		}
	}

	private static void ResetBlockGraphics(Node node)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		foreach (Node child in node.GetChildren(false))
		{
			CanvasItem val = (CanvasItem)(object)((child is CanvasItem) ? child : null);
			if (val != null)
			{
				val.SelfModulate = White;
			}
			ResetBlockGraphics(child);
		}
	}

	private static bool IsTextNode(Node node)
	{
		string name = ((object)node).GetType().Name;
		string text = ((object)node.Name).ToString();
		if (!(node is Label) && !(node is MegaLabel) && !name.Contains("Label", StringComparison.OrdinalIgnoreCase) && !text.Contains("Label", StringComparison.OrdinalIgnoreCase))
		{
			return text.Contains("Text", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}
}
