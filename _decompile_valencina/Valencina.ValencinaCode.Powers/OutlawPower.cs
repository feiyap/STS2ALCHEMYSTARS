using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Powers;

public sealed class OutlawPower : ValencinaPower, IAmmoConsumedListener, IAddDumbVariablesToPowerDescription
{
	private const int PercentPerAmmo = 2;

	private const int MaxBonusPercent = 150;

	private int _bonusPercent;

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public override int DisplayAmount => _bonusPercent;

	public decimal CurrentMultiplier => 1m + (decimal)_bonusPercent / 100m;

	public void AddDumbVariablesToPowerDescription(LocString description)
	{
		description.Add("BonusPercent", (decimal)_bonusPercent);
		description.Add("MaxBonusPercent", 150m);
	}

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Owner == null || dealer != ((PowerModel)this).Owner)
		{
			return 1m;
		}
		if (!ValuePropExtensions.IsPoweredAttack(props))
		{
			return 1m;
		}
		return CurrentMultiplier;
	}

	public async Task OnAmmoConsumedAsync(int consumed, int requested, Creature owner, Player? player, CardModel? sourceCard)
	{
		if (((PowerModel)this).Owner != null && owner == ((PowerModel)this).Owner && consumed > 0)
		{
			_bonusPercent = Math.Min(150, _bonusPercent + consumed * 2);
			((PowerModel)this).InitInternalData();
			((PowerModel)this).InvokeDisplayAmountChanged();
			await Task.CompletedTask;
		}
	}
}
