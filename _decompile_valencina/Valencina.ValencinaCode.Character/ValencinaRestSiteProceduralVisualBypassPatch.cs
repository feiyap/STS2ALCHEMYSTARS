using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Scaffolding.Characters.Patches;

namespace Valencina.ValencinaCode.Character;

[HarmonyPatch(typeof(NRestSiteRoomProceduralVisualPlaybackPatch), "Postfix")]
internal static class ValencinaRestSiteProceduralVisualBypassPatch
{
	private static bool Prefix(NRestSiteRoom __0)
	{
		if (__0.Characters.Any(delegate(NRestSiteCharacter c)
		{
			Player player = c.Player;
			if (player != null)
			{
				IRunState runState = player.RunState;
				if (((runState != null) ? new int?(runState.CurrentActIndex) : ((int?)null)) >= 3)
				{
					Player player2 = c.Player;
					return ((player2 != null) ? player2.Character : null) is Valencina;
				}
			}
			return false;
		}))
		{
			MainFile.Logger.Info("[ValencinaRestSite] Bypassed RitsuLib procedural visual playback for Valencina act 4.", 1);
			return false;
		}
		return true;
	}
}
