using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Powers;

public sealed class ScorchMarkPower : ValencinaPower
{
	private bool _echoing;

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)0;

	public override bool AllowNegative => false;

	public async Task ValencinaAfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (((PowerModel)this).Owner == null || applier != ((PowerModel)this).Owner || amount <= 0m || _echoing || power.Owner == null || power.Owner == ((PowerModel)this).Owner)
		{
			return;
		}
		int num = (int)amount;
		if (num <= 0)
		{
			return;
		}
		try
		{
			_echoing = true;
			if (power is BurnPower)
			{
				await StatusSystem.ApplyTremorAsync(power.Owner, num, cardSource, allowStarterRelicConversion: false);
			}
			else if (power is TremorPower || power is BurningTremorPower)
			{
				await StatusSystem.ApplyBurnAsync(power.Owner, num, cardSource);
			}
		}
		finally
		{
			_echoing = false;
		}
	}
}
