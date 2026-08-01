using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Cards;

public sealed class Oppression : ValencinaCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		new DynamicVar("Select", 0m),
		(DynamicVar)new EnergyVar(1)
	});

	protected override bool IsPlayable => DrawPileHasCards();

	protected override bool ShouldGlowGoldInternal => DrawPileHasCards();

	public Oppression()
		: base(0, (CardType)2, (CardRarity)3, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		List<CardModel> list = PileTypeExtensions.GetPile((PileType)1, ((CardModel)this).Owner).Cards.ToList();
		if (list.Count == 0)
		{
			return;
		}
		CardModel val3;
		if (IsCardUpgraded())
		{
			CardSelectorPrefs val = new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1);
			((CardSelectorPrefs)(ref val)).set_RequireManualConfirmation(true);
			CardSelectorPrefs val2 = val;
			val3 = (await CardSelectCmd.FromSimpleGrid(choiceContext, (IReadOnlyList<CardModel>)list, ((CardModel)this).Owner, val2)).FirstOrDefault();
		}
		else
		{
			val3 = list.FirstOrDefault();
		}
		if (val3 != null)
		{
			int energy = ValencinaCombatCardHelper.ReadEnergyCostForCard(val3);
			await CardCmd.Exhaust(choiceContext, val3, false, false);
			if (energy > 0)
			{
				await PlayerCmd.GainEnergy((decimal)energy, ((CardModel)this).Owner);
			}
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Select"].UpgradeValueBy(1m);
	}

	private bool DrawPileHasCards()
	{
		if (((CardModel)this).Owner != null)
		{
			return PileTypeExtensions.GetPile((PileType)1, ((CardModel)this).Owner).Cards.Count > 0;
		}
		return false;
	}
}
