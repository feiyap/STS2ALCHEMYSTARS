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

public sealed class AcceleratedFuturePower : ValencinaPower, IAmmoConsumedListener, IAddDumbVariablesToPowerDescription
{
	public const int Threshold = 3;

	public const int DrawPerStack = 2;

	private int _storedAmmo;

	public int ThresholdAmount => 3;

	public int DrawAmount => 2 * Math.Max(1, ((PowerModel)this).Amount);

	public int StoredAmmo => _storedAmmo;

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public void AddDumbVariablesToPowerDescription(LocString description)
	{
		description.Add("ThresholdAmount", (decimal)ThresholdAmount);
		description.Add("DrawAmount", (decimal)DrawAmount);
		description.Add("StoredAmmo", (decimal)StoredAmmo);
	}

	public async Task OnAmmoConsumedAsync(int consumed, int requested, Creature owner, Player? player, CardModel? sourceCard)
	{
		if (((PowerModel)this).Owner != null && owner == ((PowerModel)this).Owner && consumed > 0 && player != null)
		{
			_storedAmmo += consumed;
			while (_storedAmmo >= 3)
			{
				_storedAmmo -= 3;
				((PowerModel)this).Flash();
				await CardPileCmd.Draw((PlayerChoiceContext)new BlockingPlayerChoiceContext(), (decimal)DrawAmount, player, false);
			}
			((PowerModel)this).InitInternalData();
			((PowerModel)this).InvokeDisplayAmountChanged();
		}
	}
}
