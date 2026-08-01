using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Events;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(EventOption), "AddLocVars")]
internal static class ValencinaSafeEventOptionLocVarsPatch
{
	private static bool Prefix(EventOption __instance, EventModel eventModel)
	{
		if (__instance.Description == null)
		{
			return false;
		}
		if (!(eventModel is CockroachEmperorPassiveDisableEvent))
		{
			try
			{
				Player owner = eventModel.Owner;
				if (owner != null)
				{
					CharacterModel character = owner.Character;
					if (character != null)
					{
						character.AddDetailsTo(__instance.Description);
					}
				}
			}
			catch (NullReferenceException)
			{
			}
		}
		LocString description = __instance.Description;
		Player owner2 = eventModel.Owner;
		description.Add("IsMultiplayer", owner2 != null && ((IPlayerCollection)owner2.RunState).Players.Count > 1);
		return false;
	}
}
