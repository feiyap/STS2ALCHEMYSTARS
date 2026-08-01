using System;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Powers;

public sealed class OdinEyeRatioPower : ValencinaPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)2;

	public override bool AllowNegative => false;

	public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
	{
		modifiedAmount = amount;
		if (target != ((PowerModel)this).Owner || !(canonicalPower is OdinEyeRatioPower) || amount <= 0m)
		{
			return false;
		}
		modifiedAmount = Math.Max(0m, Math.Min(amount, 1m - (decimal)((PowerModel)this).Amount));
		return modifiedAmount != amount;
	}
}
