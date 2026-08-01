using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Singleton;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(MultiplayerScalingModel), "GetMultiplayerScaling")]
internal static class ValencinaAct4MultiplayerScalingPatch
{
	private static bool Prefix(EncounterModel? encounter, int actIndex, ref decimal __result)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		if (actIndex != 3)
		{
			return true;
		}
		__result = ((encounter != null && (int)encounter.RoomType == 3) ? 1.3m : 1.2m);
		return false;
	}
}
