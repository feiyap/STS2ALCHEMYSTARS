using System;
using Godot;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.addons.mega_text;
using Valencina.ValencinaCode.Character;
using Valencina.ValencinaCode.Settings;

namespace Valencina.ValencinaCode.Patches;

internal static class ValencinaWarDifficulty
{
	internal const int Level = 11;

	private const int VanillaMaxLevel = 10;

	private static bool _warChosenForPendingRun;

	private static Tween? _labelJitterTween;

	private static WeakReference<MegaLabel>? _jitteredLabel;

	internal static bool HasPendingWarChoice => _warChosenForPendingRun;

	internal static bool ShouldExpose(int vanillaMax)
	{
		if (!ValencinaModConfig.DisableAdvancedDifficultySelection)
		{
			return vanillaMax >= 10;
		}
		return false;
	}

	internal static bool IsActive(IRunState? runState)
	{
		if (runState == null)
		{
			return false;
		}
		return runState.AscensionLevel == 11;
	}

	internal static void CapturePendingRun(StartRunLobby lobby)
	{
		_warChosenForPendingRun |= lobby.Ascension == 11;
	}

	internal static bool ConsumePendingWarChoice()
	{
		bool result = ValencinaModConfig.EnableWarDifficulty || _warChosenForPendingRun;
		_warChosenForPendingRun = false;
		return result;
	}

	internal static void RefreshWarText(NAscensionPanel panel)
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		MegaLabel nodeOrNull = ((Node)panel).GetNodeOrNull<MegaLabel>(NodePath.op_Implicit("HBoxContainer/AscensionIconContainer/AscensionIcon/AscensionLevel"));
		MegaRichTextLabel nodeOrNull2 = ((Node)panel).GetNodeOrNull<MegaRichTextLabel>(NodePath.op_Implicit("HBoxContainer/AscensionDescription/Description"));
		if (nodeOrNull != null && nodeOrNull2 != null)
		{
			StopJitter();
			if (panel.Ascension != 11)
			{
				((Control)nodeOrNull).RemoveThemeColorOverride(StringName.op_Implicit("font_color"));
				return;
			}
			bool num = IsValencinaSelected((Node)(object)panel);
			string text = L("VALENCINA.war_difficulty.name", "SMOG WAR");
			string text2 = L("VALENCINA.war_difficulty.description", "The war never ended....");
			string text3 = L("VALENCINA.war_difficulty.honor_suffix", "Nor did my honor...");
			nodeOrNull.SetTextAutoSize("?");
			((Control)nodeOrNull).AddThemeColorOverride(StringName.op_Implicit("font_color"), new Color("c7c9cc"));
			string text4 = (num ? (text2 + "[color=#e04444][shake rate=22 level=7]" + text3 + "[/shake][/color]") : text2);
			nodeOrNull2.Text = "[b][color=#c7c9cc][shake rate=18 level=5]" + text + "[/shake][/color][/b]\n" + text4;
		}
	}

	private static void StopJitter()
	{
		Tween? labelJitterTween = _labelJitterTween;
		if (labelJitterTween != null)
		{
			labelJitterTween.Kill();
		}
		_labelJitterTween = null;
		WeakReference<MegaLabel>? jitteredLabel = _jitteredLabel;
		if (jitteredLabel != null && jitteredLabel.TryGetTarget(out MegaLabel target) && GodotObject.IsInstanceValid((GodotObject)(object)target))
		{
			((Control)target).RotationDegrees = 0f;
		}
		_jitteredLabel = null;
	}

	private static bool IsValencinaSelected(Node node)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			for (Node val = node; val != null; val = val.GetParent())
			{
				NCharacterSelectScreen val2 = (NCharacterSelectScreen)(object)((val is NCharacterSelectScreen) ? val : null);
				if (val2 != null)
				{
					StartRunLobby lobby = val2.Lobby;
					if (lobby != null && lobby.Players.Count > 0)
					{
						return val2.Lobby.LocalPlayer.character is Valencina.ValencinaCode.Character.Valencina;
					}
				}
			}
		}
		catch
		{
		}
		return false;
	}

	private static string L(string key, string fallback)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			string formattedText = new LocString("settings_ui", key).GetFormattedText();
			return (string.IsNullOrWhiteSpace(formattedText) || formattedText == key) ? fallback : formattedText;
		}
		catch
		{
			return fallback;
		}
	}
}
