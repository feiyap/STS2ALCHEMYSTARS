using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Utils;

public static class BreathingMethodStateHelper
{
	public static int GetAmount(Creature? owner)
	{
		return BreathingMethodService.GetAmount(owner);
	}

	public static async Task RestoreExactAsync(Creature? owner, int amount, CardModel? sourceCard = null)
	{
		if (owner != null && BreathingMethodService.GetAmount(owner) != amount)
		{
			await BreathingMethodService.RemoveAsync(owner);
			if (amount > 0)
			{
				await BreathingMethodService.GainChargesAsync(owner, amount, sourceCard);
			}
		}
	}
}
