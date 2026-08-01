using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Cards;

namespace Valencina.ValencinaCode.Relics.Rien;

public sealed class FirelightFlower : RienRelic
{
	private const int RequiredBurnCards = 4;

	private const int BonusAmount = 3;

	protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[2]
	{
		new DynamicVar("Cards", 4m),
		new DynamicVar("Amount", 3m)
	};

	public static int ModifyBurnAmount(int amount, CardModel? sourceCard)
	{
		if (amount <= 0 || ((sourceCard != null) ? sourceCard.Owner : null) == null)
		{
			return amount;
		}
		FirelightFlower relic = sourceCard.Owner.GetRelic<FirelightFlower>();
		if (relic == null)
		{
			return amount;
		}
		if (sourceCard.Owner.Deck.Cards.Count((CardModel card) => card is IBurnApplyingCard) < 4)
		{
			return amount;
		}
		((RelicModel)relic).Flash();
		return amount + 3;
	}
}
