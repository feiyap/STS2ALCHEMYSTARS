using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(NMerchantCharacter), "PlayAnimation")]
internal static class ValencinaMerchantCharacterPlayAnimationPatch
{
	private static bool Prefix(NMerchantCharacter __instance)
	{
		return !ValencinaMerchantSceneGuard.IsValencinaMerchant(__instance);
	}
}
