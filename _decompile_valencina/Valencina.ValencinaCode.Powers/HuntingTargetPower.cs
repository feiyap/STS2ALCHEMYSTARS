using System;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Cards;

namespace Valencina.ValencinaCode.Powers;

public sealed class HuntingTargetPower : ValencinaPower, IAddDumbVariablesToPowerDescription
{
	public const int DamagePercentPerStack = 25;

	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public int ActiveStacks => Math.Max(0, ((PowerModel)this).Amount);

	public void AddDumbVariablesToPowerDescription(LocString description)
	{
		int activeStacks = ActiveStacks;
		description.Add("Percent", 25m);
		description.Add("TotalPercent", (decimal)(activeStacks * 25));
	}

	public decimal GetDisposalDamageBonusPercent(CardModel? cardSource, ValueProp props)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (ActiveStacks <= 0 || !(cardSource is IDisposalAttackCard) || !ValuePropExtensions.IsPoweredAttack(props))
		{
			return 0m;
		}
		return ActiveStacks * 25;
	}
}
