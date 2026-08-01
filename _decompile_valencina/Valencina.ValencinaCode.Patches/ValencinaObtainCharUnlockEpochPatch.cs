using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Saves.Managers;
using Valencina.ValencinaCode.Character;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(ProgressSaveManager), "ObtainCharUnlockEpoch")]
internal static class ValencinaObtainCharUnlockEpochPatch
{
	private static bool Prefix(Player localPlayer)
	{
		if (localPlayer.Character is Valencina.ValencinaCode.Character.Valencina)
		{
			return false;
		}
		return true;
	}
}
