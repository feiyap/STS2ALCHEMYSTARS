using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.TreasureRelicPicking;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch]
internal static class ValencinaHandImageFightMovePatch
{
	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(typeof(NHandImage), "SetTextureToFightMove", (Type[])null, (Type[])null) ?? throw new MissingMethodException(typeof(NHandImage).FullName, "SetTextureToFightMove");
	}

	public static void Postfix(NHandImage __instance, RelicPickingFightMove move)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		ValencinaMultiplayerHandTexture.ApplyToHandImage(__instance, move);
	}
}
