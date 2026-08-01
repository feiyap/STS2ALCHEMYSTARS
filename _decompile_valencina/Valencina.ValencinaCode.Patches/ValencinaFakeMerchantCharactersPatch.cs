using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Nodes.Events.Custom;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NFakeMerchant), "AfterRoomIsLoaded")]
internal static class ValencinaFakeMerchantCharactersPatch
{
	private const float ColumnSpacing = 275f;

	private static bool Prefix(NFakeMerchant __instance)
	{
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		if (!TryGetField<List<Player>>(__instance, "_players", out List<Player> value) || !TryGetField<Control>(__instance, "_characterContainer", out Control value2) || !TryGetField<FakeMerchant>(__instance, "_event", out FakeMerchant value3) || value == null || value2 == null || value3 == null)
		{
			return true;
		}
		Player me = LocalContext.GetMe((IEnumerable<Player>)value);
		if (me != null)
		{
			value.Remove(me);
			value.Insert(0, me);
		}
		int num = Mathf.CeilToInt(Mathf.Sqrt((float)value.Count));
		for (int i = 0; i < num; i++)
		{
			float num2 = -140f * (float)i;
			for (int j = 0; j < num; j++)
			{
				int num3 = i * num + j;
				if (num3 >= value.Count)
				{
					break;
				}
				Node val = CreateMerchantVisual(value[num3]);
				GodotTreeExtensions.AddChildSafely((Node)(object)value2, val);
				((Node)value2).MoveChild(val, 0);
				SetVisualPosition(val, new Vector2(num2, -50f * (float)i));
				if (i > 0)
				{
					SetVisualModulate(val, new Color(0.5f, 0.5f, 0.5f, 1f));
				}
				num2 -= 275f;
			}
		}
		if (!value3.StartedFight)
		{
			RunWelcomeDialogue(__instance);
		}
		return false;
	}

	private static Node CreateMerchantVisual(Player player)
	{
		return PreloadManager.Cache.GetScene(player.Character.MerchantAnimPath).Instantiate((GenEditState)0);
	}

	private static void SetVisualPosition(Node node, Vector2 position)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Node2D val = (Node2D)(object)((node is Node2D) ? node : null);
		if (val == null)
		{
			Control val2 = (Control)(object)((node is Control) ? node : null);
			if (val2 != null)
			{
				val2.Position = position;
			}
		}
		else
		{
			val.Position = position;
		}
	}

	private static void SetVisualModulate(Node node, Color color)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		CanvasItem val = (CanvasItem)(object)((node is CanvasItem) ? node : null);
		if (val != null)
		{
			val.Modulate = color;
		}
	}

	private static void RunWelcomeDialogue(NFakeMerchant fakeMerchantNode)
	{
		if (AccessTools.Method(typeof(NFakeMerchant), "ShowWelcomeDialogue", (Type[])null, (Type[])null)?.Invoke(fakeMerchantNode, null) is Task task)
		{
			TaskHelper.RunSafely(task);
		}
	}

	private static bool TryGetField<T>(NFakeMerchant instance, string name, out T? value)
	{
		value = default(T);
		if (!(AccessTools.Field(typeof(NFakeMerchant), name)?.GetValue(instance) is T val))
		{
			return false;
		}
		value = val;
		return true;
	}
}
