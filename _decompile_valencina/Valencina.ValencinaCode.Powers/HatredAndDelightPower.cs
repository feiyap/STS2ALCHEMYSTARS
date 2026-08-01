using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Powers;

public sealed class HatredAndDelightPower : ValencinaPower, IAmmoConsumedListener, IAddDumbVariablesToPowerDescription
{
	private const int Threshold = 5;

	private const int EnergyPerTrigger = 2;

	private int _storedAmmo;

	public int ThresholdAmount => 5;

	public int EnergyAmount => 2 * Math.Max(1, ((PowerModel)this).Amount);

	public int StoredAmmo => _storedAmmo;

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public void AddDumbVariablesToPowerDescription(LocString description)
	{
		description.Add("ThresholdAmount", (decimal)ThresholdAmount);
		description.Add("EnergyAmount", (decimal)EnergyAmount);
		description.Add("StoredAmmo", (decimal)StoredAmmo);
	}

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		await Task.CompletedTask;
	}

	public async Task OnAmmoConsumedAsync(int consumed, int requested, Creature owner, Player? player, CardModel? sourceCard)
	{
		if (((PowerModel)this).Owner == null || owner != ((PowerModel)this).Owner || consumed <= 0)
		{
			return;
		}
		_storedAmmo += consumed;
		int stacks = Math.Max(1, ((PowerModel)this).Amount);
		while (_storedAmmo >= 5)
		{
			_storedAmmo -= 5;
			if (player != null)
			{
				await PlayerCmd.GainEnergy((decimal)(2 * stacks), player);
			}
		}
		((PowerModel)this).InitInternalData();
		((PowerModel)this).InvokeDisplayAmountChanged();
	}
}
