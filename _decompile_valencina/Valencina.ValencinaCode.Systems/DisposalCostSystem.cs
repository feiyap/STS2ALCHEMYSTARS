using System;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Cards;

namespace Valencina.ValencinaCode.Systems;

public static class DisposalCostSystem
{
	public static bool IsDisposalCard(CardModel? card)
	{
		return IsDisposalAttack(card);
	}

	public static bool IsDisposalAttack(CardModel? card)
	{
		if (card is IDisposalAttackCard)
		{
			return true;
		}
		string text = ((card != null) ? ((AbstractModel)card).Id.Entry : null);
		switch (text)
		{
		default:
			return text == "VALENCINASTS2_HATRED_FUTURE_DISPOSAL";
		case "VALENCINA-FUTURE_DISPOSAL":
		case "VALENCINASTS2-FUTURE_DISPOSAL":
		case "VALENCINA-HATRED_FUTURE_DISPOSAL":
		case "VALENCINASTS2-HATRED_FUTURE_DISPOSAL":
		case "VALENCINA_FUTURE_DISPOSAL":
		case "VALENCINASTS2_FUTURE_DISPOSAL":
		case "VALENCINA_HATRED_FUTURE_DISPOSAL":
			return true;
		}
	}

	public static bool IsAnyDisposalVariant(CardModel? card)
	{
		return IsDisposalAttack(card);
	}

	public static int GetSharedCostFor(CardModel? card)
	{
		return ReadBaseCost(card);
	}

	private static int ReadBaseCost(CardModel? card)
	{
		if (card == null)
		{
			return 0;
		}
		try
		{
			if (card.EnergyCost.CostsX)
			{
				return 0;
			}
			return Math.Max(0, card.EnergyCost.GetWithModifiers((CostModifiers)0));
		}
		catch
		{
			return 0;
		}
	}
}
