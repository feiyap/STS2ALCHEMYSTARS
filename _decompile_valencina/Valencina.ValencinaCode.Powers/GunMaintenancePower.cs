using System;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Powers;

public sealed class GunMaintenancePower : ValencinaPower, IAddDumbVariablesToPowerDescription
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public void AddDumbVariablesToPowerDescription(LocString description)
	{
		description.Add("Multiplier", (decimal)Math.Max(1, ((PowerModel)this).Amount));
	}

	public int ModifyAmmoBreathingMethodGain(int baseGain)
	{
		if (baseGain > 0)
		{
			return baseGain * Math.Max(1, ((PowerModel)this).Amount);
		}
		return 0;
	}
}
