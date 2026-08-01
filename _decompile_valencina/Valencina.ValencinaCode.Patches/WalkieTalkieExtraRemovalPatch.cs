using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Relics.Rien;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(MerchantCardRemovalEntry), "SetUsed")]
internal static class WalkieTalkieExtraRemovalPatch
{
	private static readonly ConditionalWeakTable<MerchantCardRemovalEntry, object> ExtraRemovalUsed = new ConditionalWeakTable<MerchantCardRemovalEntry, object>();

	private static bool Prefix(MerchantCardRemovalEntry __instance)
	{
		try
		{
			object? obj = AccessTools.Field(typeof(MerchantEntry), "_player")?.GetValue(__instance);
			Player val = (Player)((obj is Player) ? obj : null);
			if (val == null)
			{
				return true;
			}
			WalkieTalkie relic = val.GetRelic<WalkieTalkie>();
			if (relic == null)
			{
				return true;
			}
			if (ExtraRemovalUsed.TryGetValue(__instance, out object _))
			{
				return true;
			}
			ExtraRemovalUsed.Add(__instance, new object());
			((RelicModel)relic).Flash();
			MainFile.Logger.Info("[WalkieTalkie] Skipped marking the shop card removal as used; one extra removal granted.", 1);
			return false;
		}
		catch
		{
			return true;
		}
	}
}
