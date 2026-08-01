using System;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Powers;

public abstract class PercentValencinaPower : ValencinaPower, IAddDumbVariablesToPowerDescription
{
	public override int DisplayAmount => PercentAmount;

	public int PercentAmount => (int)Math.Round(Math.Max(0m, ((PowerModel)this).Amount), MidpointRounding.AwayFromZero);

	public virtual void AddDumbVariablesToPowerDescription(LocString description)
	{
		description.Add("Percent", (decimal)PercentAmount);
	}
}
