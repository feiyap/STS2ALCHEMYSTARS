using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Precognition;

namespace Valencina.ValencinaCode.Powers;

public sealed class FarewellPower : ValencinaPower, IAddDumbVariablesToPowerDescription
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public void AddDumbVariablesToPowerDescription(LocString description)
	{
		description.Add("Cards", (decimal)Math.Max(0, ((PowerModel)this).Amount));
	}

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		Creature owner = ((PowerModel)this).Owner;
		if (owner != null)
		{
			owner.GetPower<InstantForesightPower>()?.SetPrecognition(1);
		}
		return Task.CompletedTask;
	}

	public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (((PowerModel)this).Owner == null || ((PowerModel)this).Amount <= 0)
		{
			return;
		}
		Player owner = cardPlay.Card.Owner;
		if (((owner != null) ? owner.Creature : null) != ((PowerModel)this).Owner || (int)cardPlay.Card.Type != 1 || cardPlay.Card is IPrecognitionVirtualCounterCard)
		{
			return;
		}
		Player player = ((PowerModel)this).Owner.Player;
		if (player != null)
		{
			int num = Math.Max(0, ((PowerModel)this).Amount);
			if (num > 0)
			{
				((PowerModel)this).Flash();
				await CardPileCmd.Draw(choiceContext, (decimal)num, player, false);
			}
		}
	}
}
