using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Precognition;

namespace Valencina.ValencinaCode.Powers;

public sealed class SharpPower : ValencinaPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Owner == null || dealer != ((PowerModel)this).Owner || !(cardSource is PrecognitionJieTuCounterCard) || !ValuePropExtensions.IsPoweredAttack(props))
		{
			return 0m;
		}
		return ((PowerModel)this).Amount;
	}
}
