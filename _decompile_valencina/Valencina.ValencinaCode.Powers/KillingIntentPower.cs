using System;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;

namespace Valencina.ValencinaCode.Powers;

public sealed class KillingIntentPower : PercentValencinaPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (dealer != ((PowerModel)this).Owner || ((target != null) ? target.Monster : null) == null || !ValuePropExtensions.IsPoweredAttack(props))
		{
			return 1m;
		}
		if (!target.Monster.NextMove.Intents.Any((AbstractIntent intent) => intent is AttackIntent))
		{
			return 1m;
		}
		return 1m + Math.Max(0m, ((PowerModel)this).Amount) / 100m;
	}
}
