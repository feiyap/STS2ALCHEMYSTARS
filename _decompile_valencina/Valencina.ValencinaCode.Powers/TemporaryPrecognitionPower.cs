using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Powers;

public sealed class TemporaryPrecognitionPower : ValencinaPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public void SyncAmount(int amount)
	{
		if (amount < 0)
		{
			amount = 0;
		}
		((PowerModel)this).SetAmount(amount, false);
		((PowerModel)this).InitInternalData();
		((PowerModel)this).InvokeDisplayAmountChanged();
	}
}
