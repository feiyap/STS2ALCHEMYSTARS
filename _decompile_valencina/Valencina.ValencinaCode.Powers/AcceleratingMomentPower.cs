using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Powers;

public sealed class AcceleratingMomentPower : ValencinaPower
{
	private bool _sharingBreathingMethod;

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public async Task ValencinaAfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (_sharingBreathingMethod || ((PowerModel)this).Owner == null || !(power is BreathingMethodPower) || power.Owner != ((PowerModel)this).Owner || amount <= 0m)
		{
			return;
		}
		int intensityGain = DecodeIntensityGain(amount);
		if (intensityGain <= 0)
		{
			return;
		}
		IReadOnlyList<Creature> readOnlyList = PlayerCreaturesOnOwnerSide().ToList();
		if (readOnlyList.Count == 0)
		{
			return;
		}
		((PowerModel)this).Flash();
		_sharingBreathingMethod = true;
		try
		{
			foreach (Creature item in readOnlyList)
			{
				await BreathingMethodService.GainIntensityAsync(item, intensityGain, cardSource, (PlayerChoiceContext?)new BlockingPlayerChoiceContext());
			}
		}
		finally
		{
			_sharingBreathingMethod = false;
		}
	}

	private static int DecodeIntensityGain(decimal amount)
	{
		int num = (int)amount;
		if (num >= 10000)
		{
			int num2 = 9999;
			if ((num - 1) % num2 == 0)
			{
				return Math.Max(0, (num - 1) / num2);
			}
			return Math.Max(0, num / 10000);
		}
		return 0;
	}

	private IEnumerable<Creature> PlayerCreaturesOnOwnerSide()
	{
		Creature owner = ((PowerModel)this).Owner;
		if (((owner != null) ? owner.CombatState : null) == null)
		{
			yield break;
		}
		foreach (Player player in ((PowerModel)this).Owner.CombatState.Players)
		{
			Creature val = ((player != null) ? player.Creature : null);
			if (val != null && val.IsAlive && val.Side == ((PowerModel)this).Owner.Side && val != ((PowerModel)this).Owner)
			{
				yield return val;
			}
		}
	}
}
