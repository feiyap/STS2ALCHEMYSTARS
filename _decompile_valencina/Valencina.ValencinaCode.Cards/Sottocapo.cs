using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Compat;

namespace Valencina.ValencinaCode.Cards;

public sealed class Sottocapo : ValencinaCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords
	{
		get
		{
			foreach (CardKeyword canonicalKeyword in base.CanonicalKeywords)
			{
				yield return canonicalKeyword;
			}
			yield return (CardKeyword)1;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)new EnergyVar(2));

	public Sottocapo()
		: base(2, (CardType)2, (CardRarity)4, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		int num = ((((CardModel)this).Owner != null) ? MaxHandSizePatch.GetMaxHandSize(((CardModel)this).Owner) : 10);
		int num2 = Math.Max(0, num - CountCardsInOwnerHand() + 1);
		if (num2 > 0 && ((CardModel)this).Owner != null)
		{
			await CardPileCmd.Draw(choiceContext, (decimal)num2, ((CardModel)this).Owner, false);
		}
		if (((CardModel)this).Owner != null)
		{
			await PlayerCmd.GainEnergy(2m, ((CardModel)this).Owner);
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).EnergyCost.UpgradeBy(-1);
	}
}
