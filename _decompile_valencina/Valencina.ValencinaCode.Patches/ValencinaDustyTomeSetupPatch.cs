using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using Valencina.ValencinaCode.Cards;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(DustyTome), "SetupForPlayer")]
internal static class ValencinaDustyTomeSetupPatch
{
	private static bool Prefix(DustyTome __instance, Player player)
	{
		if (!OrobasValencinaPatch.IsValencina(player))
		{
			return true;
		}
		__instance.AncientCard = ((AbstractModel)ModelDb.Card<Disposal>()).Id;
		return false;
	}
}
