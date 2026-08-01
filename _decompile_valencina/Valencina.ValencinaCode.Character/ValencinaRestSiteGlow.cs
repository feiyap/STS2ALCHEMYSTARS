using System;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.RestSite;

namespace Valencina.ValencinaCode.Character;

internal static class ValencinaRestSiteGlow
{
	private const string GlowNodePath = "ControlRoot/VisualOffset/FireGlow";

	private const string BodyNodePath = "ControlRoot/VisualOffset/Body";

	private const string ProceduralGlowNodePath = "ControlRoot/FireGlow";

	private const string ProceduralBodyNodePath = "ControlRoot/Visuals";

	private static readonly Color GlowColor = new Color(1f, 1f, 1f, 0.36f);

	private static readonly Vector2 ProceduralPosition = new Vector2(185f, -55f);

	private static readonly Vector2 ProceduralScale = new Vector2(1.03f, 1.03f);

	internal static void AddTo(NRestSiteCharacter character)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (IsValencinaRestSiteCharacter(character))
		{
			Sprite2D val = ((Node)character).GetNodeOrNull<Sprite2D>(NodePath.op_Implicit("ControlRoot/VisualOffset/FireGlow")) ?? ((Node)character).GetNodeOrNull<Sprite2D>(NodePath.op_Implicit("ControlRoot/FireGlow")) ?? CreateMissingGlow(character);
			if (val != null)
			{
				AlignGlowWithBody(character, val);
				((CanvasItem)val).Modulate = GlowColor;
				((CanvasItem)val).Visible = true;
				StartGlowLoop(val);
			}
		}
	}

	private static bool IsValencinaRestSiteCharacter(NRestSiteCharacter character)
	{
		try
		{
			Player player = character.Player;
			if (((player != null) ? player.Character : null) is Valencina)
			{
				return true;
			}
			Player player2 = character.Player;
			object obj;
			if (player2 == null)
			{
				obj = null;
			}
			else
			{
				CharacterModel character2 = player2.Character;
				obj = ((character2 != null) ? character2.RestSiteAnimPath : null);
			}
			string text = (string)obj;
			if (!string.IsNullOrWhiteSpace(text) && text.Contains("rest_site_valencina", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		catch
		{
		}
		Sprite2D? obj3 = FindBody(character);
		object obj4;
		if (obj3 == null)
		{
			obj4 = null;
		}
		else
		{
			Texture2D texture = obj3.Texture;
			obj4 = ((texture != null) ? ((Resource)texture).ResourcePath : null);
		}
		return (string?)obj4 == "res://Valencina/images/charui/rest_site_valencina_body.png";
	}

	private static Sprite2D? CreateMissingGlow(NRestSiteCharacter character)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		Sprite2D val = FindBody(character);
		Node val2 = ((val != null) ? ((Node)val).GetParent() : null);
		if (val == null || val2 == null)
		{
			return null;
		}
		Texture2D val3 = ResourceLoader.Load<Texture2D>("res://Valencina/images/charui/rest_site_valencina_fire_glow.png", (string)null, (CacheMode)1);
		if (val3 == null)
		{
			MainFile.Logger.Warn("[ValencinaRestSite] Fire glow texture missing: res://Valencina/images/charui/rest_site_valencina_fire_glow.png", 1);
			return null;
		}
		Sprite2D val4 = new Sprite2D
		{
			Name = StringName.op_Implicit("FireGlow"),
			Texture = val3,
			ZAsRelative = true,
			ZIndex = GetGlowZIndex(val)
		};
		val2.AddChild((Node)(object)val4, false, (InternalMode)0);
		AlignGlowWithBody(character, val4);
		return val4;
	}

	private static Sprite2D? FindBody(NRestSiteCharacter character)
	{
		return ((Node)character).GetNodeOrNull<Sprite2D>(NodePath.op_Implicit("ControlRoot/VisualOffset/Body")) ?? ((Node)character).GetNodeOrNull<Sprite2D>(NodePath.op_Implicit("ControlRoot/Visuals"));
	}

	private static void AlignGlowWithBody(NRestSiteCharacter character, Sprite2D fireGlow)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		Sprite2D val = FindBody(character);
		if (val != null)
		{
			((Node2D)fireGlow).Position = ((((Node2D)val).Position == Vector2.Zero && ((Node)val).Name == StringName.op_Implicit("Visuals")) ? ProceduralPosition : ((Node2D)val).Position);
			((Node2D)fireGlow).Scale = ((((Node2D)val).Scale == Vector2.One && ((Node)val).Name == StringName.op_Implicit("Visuals")) ? ProceduralScale : ((Node2D)val).Scale);
			fireGlow.FlipH = val.FlipH || ((Node)val).Name == StringName.op_Implicit("Visuals");
			((CanvasItem)fireGlow).ZAsRelative = true;
			((CanvasItem)fireGlow).ZIndex = GetGlowZIndex(val);
			KeepGlowAfterBody(val, fireGlow);
		}
	}

	private static int GetGlowZIndex(Sprite2D body)
	{
		return ((CanvasItem)body).ZIndex;
	}

	private static void KeepGlowAfterBody(Sprite2D body, Sprite2D fireGlow)
	{
		Node parent = ((Node)body).GetParent();
		if (parent != null && ((Node)fireGlow).GetParent() == parent)
		{
			int index = ((Node)body).GetIndex(false);
			if (((Node)fireGlow).GetIndex(false) <= index)
			{
				parent.MoveChild((Node)(object)fireGlow, index + 1);
			}
		}
	}

	private static void StartGlowLoop(Sprite2D fireGlow)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if (!(bool)((GodotObject)fireGlow).GetMeta(StringName.op_Implicit("ValencinaGlowLoopStarted"), Variant.op_Implicit(false)))
		{
			((GodotObject)fireGlow).SetMeta(StringName.op_Implicit("ValencinaGlowLoopStarted"), Variant.op_Implicit(true));
			Tween obj = ((Node)fireGlow).CreateTween();
			obj.SetLoops(0);
			obj.TweenProperty((GodotObject)(object)fireGlow, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(0.42f), 2.799999952316284).SetTrans((TransitionType)1).SetEase((EaseType)2);
			obj.TweenProperty((GodotObject)(object)fireGlow, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(0.32f), 3.200000047683716).SetTrans((TransitionType)1).SetEase((EaseType)2);
		}
	}
}
