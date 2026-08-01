using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Powers;

public sealed class CrystalClearPower : ValencinaPower, IAddDumbVariablesToPowerDescription, IBreathingMethodConsumedListener
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public void AddDumbVariablesToPowerDescription(LocString description)
	{
		description.Add("Cards", (decimal)Math.Max(0, ((PowerModel)this).Amount));
	}

	public async Task OnBreathingMethodConsumedAsync(PlayerChoiceContext choiceContext, int consumed, Creature owner, CardModel? sourceCard)
	{
		if (((PowerModel)this).Owner != null && owner == ((PowerModel)this).Owner && ((PowerModel)this).Amount > 0 && ((PowerModel)this).Owner.Player != null)
		{
			((PowerModel)this).Flash();
			await CardPileCmd.Draw(choiceContext, (decimal)Math.Max(0, ((PowerModel)this).Amount), ((PowerModel)this).Owner.Player, false);
		}
	}
}
