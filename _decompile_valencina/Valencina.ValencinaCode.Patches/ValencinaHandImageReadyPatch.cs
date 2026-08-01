using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NHandImage), "_Ready")]
internal static class ValencinaHandImageReadyPatch
{
	public static void Postfix(NHandImage __instance)
	{
		ValencinaMultiplayerHandTexture.ApplyToHandImage(__instance);
	}
}
