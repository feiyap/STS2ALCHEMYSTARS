using System;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Powers;

public sealed class EmptyChamberPower : PercentValencinaPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Owner == null || dealer != ((PowerModel)this).Owner || !ValuePropExtensions.IsPoweredAttack(props) || AmmoSystem.CurrentAmmo(((PowerModel)this).Owner) > 0)
		{
			return 1m;
		}
		return 1m + Math.Max(0m, ((PowerModel)this).Amount) / 100m;
	}
}
