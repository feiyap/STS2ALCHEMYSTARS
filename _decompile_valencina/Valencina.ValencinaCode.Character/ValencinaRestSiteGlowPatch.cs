using System;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Valencina.ValencinaCode.Character;

[HarmonyPatch(typeof(NRestSiteRoom), "_Ready")]
internal static class ValencinaRestSiteGlowPatch
{
	private static void Postfix(NRestSiteRoom __instance)
	{
		if (__instance.Characters.Any(delegate(NRestSiteCharacter character)
		{
			Player player = character.Player;
			return ((player != null) ? player.Character : null) is Valencina;
		}))
		{
			MainFile.Logger.Info("[ValencinaRestSite] NRestSiteRoom postfix begin.", 1);
		}
		foreach (NRestSiteCharacter character in __instance.Characters)
		{
			ValencinaRestSiteGlow.AddTo(character);
		}
		if (__instance.Characters.Any(delegate(NRestSiteCharacter character)
		{
			Player player = character.Player;
			return ((player != null) ? player.Character : null) is Valencina;
		}))
		{
			MainFile.Logger.Info("[ValencinaRestSite] NRestSiteRoom postfix complete.", 1);
		}
	}

	[HarmonyFinalizer]
	private static Exception? Finalizer(NRestSiteRoom __instance, Exception? __exception)
	{
		if (__exception == null)
		{
			return null;
		}
		if (__instance.Characters.Any(delegate(NRestSiteCharacter c)
		{
			Player player = c.Player;
			return ((player != null) ? player.Character : null) is Valencina;
		}))
		{
			MainFile.Logger.Warn("[ValencinaRestSite] Suppressed NRestSiteRoom._Ready exception for Valencina: " + __exception.GetType().Name + ": " + __exception.Message, 1);
			return null;
		}
		return __exception;
	}
}
