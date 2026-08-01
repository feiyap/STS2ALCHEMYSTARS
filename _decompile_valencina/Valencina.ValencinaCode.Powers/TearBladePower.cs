using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Valencina.ValencinaCode.Powers;

public sealed class TearBladePower : ValencinaPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (dealer != ((PowerModel)this).Owner || ((PowerModel)this).Amount < 2 || !ValuePropExtensions.IsPoweredAttack(props))
		{
			return 1m;
		}
		return 1.25m;
	}

	public override int ModifyAttackHitCount(AttackCommand attack, int hitCount)
	{
		if (attack.Attacker == ((PowerModel)this).Owner && ((PowerModel)this).Amount >= 3)
		{
			return hitCount + 1;
		}
		return hitCount;
	}
}
