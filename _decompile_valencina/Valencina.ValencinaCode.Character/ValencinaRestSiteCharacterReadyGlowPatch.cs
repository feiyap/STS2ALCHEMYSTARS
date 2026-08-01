using System;
using System.Collections;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Random;

namespace Valencina.ValencinaCode.Character;

[HarmonyPatch(typeof(NRestSiteCharacter), "_Ready")]
internal static class ValencinaRestSiteCharacterReadyGlowPatch
{
	private static void Postfix(NRestSiteCharacter __instance)
	{
		ValencinaRestSiteGlow.AddTo(__instance);
	}

	[HarmonyFinalizer]
	private static Exception? Finalizer(NRestSiteCharacter __instance, Exception? __exception)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		if (__exception == null)
		{
			return null;
		}
		if (__exception is InvalidOperationException)
		{
			Player player = __instance.Player;
			if (((player != null) ? player.Character : null) is Valencina && __instance.Player.RunState.CurrentActIndex >= 3)
			{
				if (!string.Equals(__exception.Message, "Unexpected act", StringComparison.Ordinal))
				{
					return __exception;
				}
				try
				{
					foreach (Node2D item in ((IEnumerable)((Node)__instance).GetChildren(false)).OfType<Node2D>())
					{
						if (!(((GodotObject)item).GetClass() != "SpineSprite"))
						{
							MegaTrackEntry val = new MegaSprite(Variant.op_Implicit((GodotObject)(object)item)).GetAnimationState().SetAnimation("glory_loop", true, 0);
							if (val != null)
							{
								val.SetTrackTime(val.GetAnimationEnd() * Rng.Chaotic.NextFloat(1f));
							}
						}
					}
					ConnectRestSiteHitbox(__instance);
					MainFile.Logger.Info("[ValencinaRestSite] Recovered Act4 rest-site character _Ready with glory_loop.", 1);
					return null;
				}
				catch (Exception value)
				{
					MainFile.Logger.Error($"[ValencinaRestSite] Act4 rest-site recovery failed: {value}", 1);
					return __exception;
				}
			}
		}
		return __exception;
	}

	private static void ConnectRestSiteHitbox(NRestSiteCharacter character)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		if (character.Hitbox != null)
		{
			Action action = AccessTools.MethodDelegate<Action>(AccessTools.Method(typeof(NRestSiteCharacter), "OnFocus", (Type[])null, (Type[])null), (object)character, true, (Type[])null);
			Action action2 = AccessTools.MethodDelegate<Action>(AccessTools.Method(typeof(NRestSiteCharacter), "OnUnfocus", (Type[])null, (Type[])null), (object)character, true, (Type[])null);
			((GodotObject)character.Hitbox).Connect(SignalName.FocusEntered, Callable.From(action), 0u);
			((GodotObject)character.Hitbox).Connect(SignalName.FocusExited, Callable.From(action2), 0u);
			((GodotObject)character.Hitbox).Connect(SignalName.MouseEntered, Callable.From(action), 0u);
			((GodotObject)character.Hitbox).Connect(SignalName.MouseExited, Callable.From(action2), 0u);
		}
	}
}
