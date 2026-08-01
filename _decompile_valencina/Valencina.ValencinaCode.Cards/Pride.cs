using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Cards;

public sealed class Pride : ValencinaCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Amount", 1m));

	public Pride()
		: base(1, (CardType)2, (CardRarity)3, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		List<CardModel> cards = PileTypeExtensions.GetPile((PileType)2, ((CardModel)this).Owner).Cards.Where((CardModel card) => (object)card != this).ToList();
		foreach (CardModel item in cards)
		{
			await CardCmd.Exhaust(choiceContext, item, false, false);
		}
		decimal num = (decimal)cards.Count * ((CardModel)this).DynamicVars["Amount"].BaseValue;
		if (num > 0m)
		{
			await BreathingMethodService.GainChargesAsync(((CardModel)this).Owner.Creature, (int)num, (CardModel?)(object)this, choiceContext);
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Amount"].UpgradeValueBy(1m);
	}
}
