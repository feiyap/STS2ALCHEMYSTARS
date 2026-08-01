using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(ArchaicTooth), "AfterObtained")]
internal static class ValencinaArchaicToothAfterObtainedPatch
{
	private static bool Prefix(ArchaicTooth __instance, ref Task __result)
	{
		if (!OrobasValencinaPatch.IsValencina(((RelicModel)__instance).Owner))
		{
			return true;
		}
		__result = AfterObtainedForValencina(__instance);
		return false;
	}

	private static async Task AfterObtainedForValencina(ArchaicTooth relic)
	{
		CardModel val = OrobasValencinaPatch.FindEnduredHumiliation(((RelicModel)relic).Owner);
		if (val != null)
		{
			await CardCmd.Transform(val, OrobasValencinaPatch.CreateFamilyShameReplacement(val), (CardPreviewStyle)1);
		}
	}
}
