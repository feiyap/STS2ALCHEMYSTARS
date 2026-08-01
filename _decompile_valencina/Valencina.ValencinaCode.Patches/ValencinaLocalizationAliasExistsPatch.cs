using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(LocTable), "HasEntry")]
internal static class ValencinaLocalizationAliasExistsPatch
{
	private static void Prefix(ref string key)
	{
		key = ValencinaLocalizationAliasPatch.ToLegacyLocKey(key);
	}
}
