using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Cards;

namespace Valencina.ValencinaCode.Relics.Rien;

public sealed class Textbook : RienRelic
{
	public override bool HasUponPickupEffect => true;

	public override async Task AfterObtained()
	{
		if (((RelicModel)this).Owner == null)
		{
			return;
		}
		List<CardModel> list = ((RelicModel)this).Owner.Deck.Cards.Where((CardModel card) => (card is Lucio || card is Vagrant) ? true : false).ToList();
		if (list.Count == 0)
		{
			CardCmd.PreviewCardPileAdd(await CardPileCmd.Add((CardModel)(object)((ICardScope)((RelicModel)this).Owner.RunState).CreateCard<Vagrant>(((RelicModel)this).Owner), (PileType)6, (CardPilePosition)1, (AbstractModel)null, false), 2f, (CardPreviewStyle)1);
			return;
		}
		foreach (CardModel item in list)
		{
			CardModel val = CreateShinReplacement(item);
			await CardCmd.Transform(item, val, (CardPreviewStyle)1);
		}
	}

	private static CardModel CreateShinReplacement(CardModel sourceCard)
	{
		Player owner = sourceCard.Owner;
		Shin shin = ((ICardScope)owner.RunState).CreateCard<Shin>(owner);
		if (sourceCard.IsUpgraded)
		{
			CardCmd.Upgrade((CardModel)(object)shin, (CardPreviewStyle)1);
		}
		CardCmd.ClearEnchantment((CardModel)(object)shin);
		return (CardModel)(object)shin;
	}
}
