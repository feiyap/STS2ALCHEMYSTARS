using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NHoverTipCardContainer), "Add")]
public static class CounterPreviewHoverInspectPatch
{
	private static void Postfix(NHoverTipCardContainer __instance, CardHoverTip cardTip)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if (!IsCounterPreview(cardTip.Card))
		{
			return;
		}
		Control? obj = ((IEnumerable)((Node)__instance).GetChildren(false)).OfType<Control>().LastOrDefault();
		NCard val = ((obj != null) ? ((Node)obj).GetNodeOrNull<NCard>(NodePath.op_Implicit("%Card")) : null);
		if (val != null)
		{
			((Control)val).MouseFilter = (MouseFilterEnum)0;
			((GodotObject)val).Connect(SignalName.GuiInput, Callable.From<InputEvent>((Action<InputEvent>)delegate(InputEvent input)
			{
				OnPreviewCardInput(input, cardTip.Card);
			}), 4u);
		}
	}

	private static void OnPreviewCardInput(InputEvent input, CardModel card)
	{
		if (!input.IsActionPressed(MegaInput.select, false, false) && !input.IsActionPressed(MegaInput.accept, false, false))
		{
			return;
		}
		NGame instance = NGame.Instance;
		if (instance != null)
		{
			NInspectCardScreen inspectCardScreen = instance.GetInspectCardScreen();
			if (inspectCardScreen != null)
			{
				inspectCardScreen.Open(new List<CardModel> { card }, 0, false);
			}
		}
	}

	private static bool IsCounterPreview(CardModel card)
	{
		string entry = ((AbstractModel)card).Id.Entry;
		if (!entry.EndsWith("COUNTER_PREVIEW_LV0") && !entry.EndsWith("COUNTER_PREVIEW_LV1") && !entry.EndsWith("COUNTER_PREVIEW_LV2"))
		{
			return entry.EndsWith("COUNTER_PREVIEW_LV3");
		}
		return true;
	}
}
