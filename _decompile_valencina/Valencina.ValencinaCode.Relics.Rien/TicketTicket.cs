using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Relics.Rien;

public sealed class TicketTicket : RienRelic
{
	private const string MaxHpLossKey = "MaxHpLoss";

	public override bool HasUponPickupEffect => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[1]
	{
		new DynamicVar("MaxHpLoss", 10m)
	};

	public override async Task AfterObtained()
	{
		List<CardModel> list = PileTypeExtensions.GetPile((PileType)6, ((RelicModel)this).Owner).Cards.Where((CardModel card) => (int)card.Type == 5).ToList();
		if (list.Count > 0)
		{
			await CardPileCmd.RemoveFromDeck((IReadOnlyList<CardModel>)list, true);
		}
		await CreatureCmd.LoseMaxHp((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), ((RelicModel)this).Owner.Creature, ((RelicModel)this).DynamicVars["MaxHpLoss"].BaseValue, false);
	}
}
