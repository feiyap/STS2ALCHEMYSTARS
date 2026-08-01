using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Cards;
using Valencina.ValencinaCode.Character;
using Valencina.ValencinaCode.Relics;

namespace Valencina.ValencinaCode.Patches;

internal static class OrobasValencinaPatch
{
	internal static bool IsValencina(Player? player)
	{
		return ((player != null) ? player.Character : null) is Valencina.ValencinaCode.Character.Valencina;
	}

	internal static CardModel? FindEnduredHumiliation(Player? player)
	{
		if (player == null)
		{
			return null;
		}
		ModelId starterCardId = ((AbstractModel)ModelDb.Card<EnduredHumiliation>()).Id;
		return ((IEnumerable<CardModel>)player.Deck.Cards).FirstOrDefault((Func<CardModel, bool>)((CardModel card) => ((AbstractModel)card).Id == starterCardId));
	}

	internal static CardModel CreateFamilyShameReplacement(CardModel starterCard)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		Player owner = starterCard.Owner;
		FamilyShame familyShame = ((ICardScope)owner.RunState).CreateCard<FamilyShame>(owner);
		if (starterCard.IsUpgraded)
		{
			CardCmd.Upgrade((CardModel)(object)familyShame, (CardPreviewStyle)1);
		}
		if (starterCard.Enchantment != null)
		{
			EnchantmentModel val = (EnchantmentModel)((AbstractModel)starterCard.Enchantment).MutableClone();
			CardCmd.Enchant(val, (CardModel)(object)familyShame, (decimal)val.Amount);
		}
		return (CardModel)(object)familyShame;
	}

	internal static void MarkRelicDiscovered(Player? player, RelicModel relic)
	{
		if (player != null && !player.DiscoveredRelics.Contains(((AbstractModel)relic).Id))
		{
			player.DiscoveredRelics.Add(((AbstractModel)relic).Id);
		}
	}

	internal static void AddForesightEyeRefinement(Dictionary<ModelId, RelicModel>? upgrades)
	{
		if (upgrades != null)
		{
			ModelId id = ((AbstractModel)ModelDb.Relic<ImperfectForesightEye>()).Id;
			if (!upgrades.ContainsKey(id))
			{
				upgrades[id] = (RelicModel)(object)ModelDb.Relic<CompleteForesightEye>();
			}
		}
	}
}
