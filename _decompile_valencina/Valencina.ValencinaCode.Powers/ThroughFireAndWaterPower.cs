using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Powers;

public sealed class ThroughFireAndWaterPower : ValencinaPower, IAmmoConsumedListener, IAddDumbVariablesToPowerDescription
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public void AddDumbVariablesToPowerDescription(LocString description)
	{
		description.Add("Cost", (decimal)Math.Max(0, ((PowerModel)this).Amount));
	}

	public async Task OnAmmoConsumedAsync(int consumed, int requested, Creature owner, Player? player, CardModel? sourceCard)
	{
		if (((PowerModel)this).Owner != null && owner == ((PowerModel)this).Owner && consumed > 0)
		{
			((PowerModel)this).Flash();
			InstantForesightPower power = ((PowerModel)this).Owner.GetPower<InstantForesightPower>();
			if (power != null)
			{
				await power.SpendPrecognitionForEffectAsync((PlayerChoiceContext)new BlockingPlayerChoiceContext(), Math.Max(0, (int)Math.Ceiling((decimal)((PowerModel)this).Amount)), keepAtLeastOne: true);
			}
		}
	}
}
