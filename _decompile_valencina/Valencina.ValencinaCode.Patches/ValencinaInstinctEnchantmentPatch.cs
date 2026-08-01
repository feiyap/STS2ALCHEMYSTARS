using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Cards;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(Instinct), "EnchantDamageMultiplicative")]
internal static class ValencinaInstinctEnchantmentPatch
{
	private static void Postfix(Instinct __instance, decimal originalDamage, ValueProp props, ref decimal __result)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Invalid comparison between Unknown and I4
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (!(__result != 1m) && !(originalDamage <= 0m) && (int)((EnchantmentModel)__instance).Status == 0)
		{
			CardModel card = ((EnchantmentModel)__instance).Card;
			if (card is ValencinaCard && (int)card.Type == 1 && !((Enum)props).HasFlag((Enum)(object)(ValueProp)4))
			{
				__result = 2m;
			}
		}
	}
}
