using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Powers;

public sealed class FutureSightPower : ValencinaPower, IAmmoConsumedListener, IAddDumbVariablesToPowerDescription
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public void AddDumbVariablesToPowerDescription(LocString description)
	{
		description.Add("Amount", (decimal)Math.Max(0, ((PowerModel)this).Amount));
	}

	public Task OnAmmoConsumedAsync(int consumed, int requested, Creature owner, Player? player, CardModel? sourceCard)
	{
		if (((PowerModel)this).Owner == null || owner != ((PowerModel)this).Owner || consumed <= 0 || ((PowerModel)this).Amount <= 0)
		{
			return Task.CompletedTask;
		}
		int amount = consumed * Math.Max(0, ((PowerModel)this).Amount);
		((PowerModel)this).Owner.GetPower<InstantForesightPower>()?.GainTemporaryDodgeThreshold(amount);
		((PowerModel)this).Flash();
		return Task.CompletedTask;
	}
}
