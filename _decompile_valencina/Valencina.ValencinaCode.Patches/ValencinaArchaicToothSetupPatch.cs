using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(ArchaicTooth), "SetupForPlayer")]
internal static class ValencinaArchaicToothSetupPatch
{
	private static bool Prefix(ArchaicTooth __instance, Player player, ref bool __result)
	{
		if (!OrobasValencinaPatch.IsValencina(player))
		{
			return true;
		}
		CardModel val = OrobasValencinaPatch.FindEnduredHumiliation(player);
		if (val == null)
		{
			__result = false;
			return false;
		}
		CardModel val2 = OrobasValencinaPatch.CreateFamilyShameReplacement(val);
		__instance.SetupForTests(val.ToSerializable(), val2.ToSerializable());
		__result = true;
		return false;
	}
}
