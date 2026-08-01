using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Compat;

namespace Valencina.ValencinaCode.Powers;

public sealed class SoWeakPower : ValencinaPower, ITremorAppliedListener, IAddDumbVariablesToPowerDescription
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public void AddDumbVariablesToPowerDescription(LocString description)
	{
		description.Add("Amount", (decimal)Math.Max(0, ((PowerModel)this).Amount));
	}

	public async Task OnTremorAppliedAsync(PlayerChoiceContext choiceContext, Creature target, int amount, CardModel? sourceCard)
	{
		if (!target.IsDead && amount > 0)
		{
			((PowerModel)this).Flash();
			await CommonActions.Apply<HighTemperatureStrengthDownPower>(choiceContext, target, sourceCard, (decimal)Math.Max(0, ((PowerModel)this).Amount), silent: false);
		}
	}
}
