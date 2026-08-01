using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Cards;

public sealed class TakeAim : ValencinaCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Amount", 5m));

	public TakeAim()
		: base(1, (CardType)2, (CardRarity)3, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		Creature target = play.Target;
		if (target != null)
		{
			int num = Math.Max(0, CountCardsPlayedThisTurn() - 1);
			int num2 = (1 + num) * ((CardModel)this).DynamicVars["Amount"].IntValue;
			if (num2 > 0)
			{
				await StatusSystem.ApplyTremorAsync(target, num2, (CardModel?)(object)this, allowStarterRelicConversion: true, choiceContext);
			}
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Amount"].UpgradeValueBy(2m);
	}
}
