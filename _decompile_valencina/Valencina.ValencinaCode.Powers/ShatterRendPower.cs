using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Powers;

public sealed class ShatterRendPower : ValencinaPower, ITremorDetonatedListener, IAddDumbVariablesToPowerDescription
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)0;

	public override bool AllowNegative => false;

	public override int DisplayAmount => Math.Max(0, ((PowerModel)this).Amount);

	public void AddDumbVariablesToPowerDescription(LocString description)
	{
		description.Add("Amount", (decimal)Math.Max(0, ((PowerModel)this).Amount));
	}

	public async Task OnTremorDetonatedAsync(PlayerChoiceContext choiceContext, Creature target, int amount, CardModel? sourceCard)
	{
		if (((PowerModel)this).Owner != null && amount > 0)
		{
			int num = Math.Max(0, ((PowerModel)this).Amount);
			if (num != 0 && ((PowerModel)this).Owner.Player != null)
			{
				((PowerModel)this).Flash();
				await CardPileCmd.Draw(choiceContext, (decimal)num, ((PowerModel)this).Owner.Player, false);
			}
		}
	}
}
