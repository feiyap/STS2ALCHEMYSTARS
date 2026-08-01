using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Utils;

public static class BreathingMethodService
{
	public static int GetAmount(Creature? owner)
	{
		return GetCharges(owner);
	}

	public static int GetCharges(Creature? owner)
	{
		return CreaturePowerAccess.Find<BreathingMethodPower>(owner)?.Charges ?? 0;
	}

	public static int GetIntensity(Creature? owner)
	{
		return CreaturePowerAccess.Find<BreathingMethodPower>(owner)?.Intensity ?? 0;
	}

	public static async Task ApplyAsync(Creature? owner, int amount, CardModel? sourceCard = null, PlayerChoiceContext? choiceContext = null)
	{
		await GainIntensityAsync(owner, amount, sourceCard, choiceContext);
	}

	public static async Task GainIntensityAsync(Creature? owner, int amount, CardModel? sourceCard = null, PlayerChoiceContext? choiceContext = null)
	{
		await GainIntensityAndChargesAsync(owner, amount, 0, sourceCard, choiceContext);
	}

	public static async Task GainIntensityAndChargesAsync(Creature? owner, int intensity, int charges, CardModel? sourceCard = null, PlayerChoiceContext? choiceContext = null)
	{
		intensity = Math.Max(0, intensity);
		charges = Math.Max(0, charges);
		if (owner == null || (intensity <= 0 && charges <= 0))
		{
			return;
		}
		PlayerChoiceContext val = (PlayerChoiceContext)(((object)choiceContext) ?? ((object)new BlockingPlayerChoiceContext()));
		BreathingMethodPower breathingMethodPower = CreaturePowerAccess.Find<BreathingMethodPower>(owner);
		int num;
		if (breathingMethodPower != null && breathingMethodPower.Charges > 0)
		{
			num = intensity * 10000 + charges;
			using (BreathingMethodPower.SuppressLegacyRawAmountConversion())
			{
				await PowerCmd.ModifyAmount(val, (PowerModel)(object)breathingMethodPower, (decimal)num, owner, sourceCard, false);
				return;
			}
		}
		int charges2 = ((charges > 0) ? charges : ((intensity > 0) ? 1 : 0));
		num = BreathingMethodPower.Encode(intensity, charges2);
		if (sourceCard != null)
		{
			await CommonActions.ApplySelf<BreathingMethodPower>(val, sourceCard, (decimal)num, silent: false);
		}
		else
		{
			await CompatPowerCmd.Apply<BreathingMethodPower>(val, owner, (decimal)num, owner, (CardModel?)null, silent: false);
		}
	}

	public static async Task GainChargesAsync(Creature? owner, int amount, CardModel? sourceCard = null, PlayerChoiceContext? choiceContext = null)
	{
		if (owner == null || amount <= 0)
		{
			return;
		}
		PlayerChoiceContext val = (PlayerChoiceContext)(((object)choiceContext) ?? ((object)new BlockingPlayerChoiceContext()));
		BreathingMethodPower breathingMethodPower = CreaturePowerAccess.Find<BreathingMethodPower>(owner);
		if (breathingMethodPower != null && breathingMethodPower.Charges > 0)
		{
			using (BreathingMethodPower.SuppressLegacyRawAmountConversion())
			{
				await PowerCmd.ModifyAmount(val, (PowerModel)(object)breathingMethodPower, (decimal)amount, owner, sourceCard, false);
				return;
			}
		}
		int num = BreathingMethodPower.Encode(1, amount);
		if (sourceCard != null)
		{
			await CommonActions.ApplySelf<BreathingMethodPower>(val, sourceCard, (decimal)num, silent: false);
		}
		else
		{
			await CompatPowerCmd.Apply<BreathingMethodPower>(val, owner, (decimal)num, owner, (CardModel?)null, silent: false);
		}
	}

	public static async Task RemoveAsync(Creature? owner)
	{
		BreathingMethodPower breathingMethodPower = CreaturePowerAccess.Find<BreathingMethodPower>(owner);
		if (breathingMethodPower != null)
		{
			await PowerCmd.Remove((PowerModel)(object)breathingMethodPower);
		}
	}
}
