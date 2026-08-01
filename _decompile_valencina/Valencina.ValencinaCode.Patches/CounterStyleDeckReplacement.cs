using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Cards;
using Valencina.ValencinaCode.Relics;

namespace Valencina.ValencinaCode.Patches;

internal static class CounterStyleDeckReplacement
{
	public static bool TryReplace(CardModel? card, out CardPileAddResult result)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		result = default(CardPileAddResult);
		if (card is ICounterStyleCard counterStyleCard)
		{
			Player owner = card.Owner;
			BernoullitMemory bernoullitMemory = ((owner != null) ? owner.GetRelic<BernoullitMemory>() : null);
			if (bernoullitMemory != null)
			{
				bernoullitMemory.ReplaceCounterStyle(counterStyleCard, card.CurrentUpgradeLevel > 0);
				result = new CardPileAddResult
				{
					success = true,
					cardAdded = card,
					oldPile = card.Pile,
					modifyingModels = null
				};
				return true;
			}
		}
		return false;
	}
}
