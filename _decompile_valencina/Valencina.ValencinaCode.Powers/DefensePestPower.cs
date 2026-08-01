using System;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Valencina.ValencinaCode.Powers;

public sealed class DefensePestPower : ValencinaPower
{
	private const int MaxDefensePest = 20;

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Invalid comparison between Unknown and I4
		modifiedAmount = amount;
		if (target == ((PowerModel)this).Owner && canonicalPower is DefensePestPower && amount > 0m)
		{
			int num = Math.Max(0, 20 - ((PowerModel)this).Amount);
			modifiedAmount = Math.Min(amount, num);
			return modifiedAmount != amount;
		}
		if (target != ((PowerModel)this).Owner || ((PowerModel)this).Amount <= 0 || amount <= 0m || (int)canonicalPower.Type != 2)
		{
			return false;
		}
		if (applier == ((PowerModel)this).Owner && (canonicalPower is WeakPower || canonicalPower is VulnerablePower))
		{
			return false;
		}
		int num2 = Math.Min(((PowerModel)this).Amount, (int)Math.Ceiling(amount));
		((PowerModel)this).SetAmount(Math.Max(0, ((PowerModel)this).Amount - num2), false);
		modifiedAmount = Math.Max(0m, amount - (decimal)num2);
		((PowerModel)this).Flash();
		return true;
	}
}
