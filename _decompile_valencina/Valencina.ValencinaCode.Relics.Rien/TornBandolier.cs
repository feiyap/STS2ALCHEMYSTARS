using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Cards;

namespace Valencina.ValencinaCode.Relics.Rien;

public sealed class TornBandolier : RienRelic
{
	private const string AmountKey = "Amount";

	protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[1]
	{
		new DynamicVar("Amount", 2m)
	};

	public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Invalid comparison between Unknown and I4
		Player owner = ((RelicModel)this).Owner;
		if (((owner != null) ? owner.Creature : null) == null || dealer != ((RelicModel)this).Owner.Creature || !ValuePropExtensions.IsPoweredAttack(props))
		{
			return 0m;
		}
		if (!(cardSource is ValencinaCard { SpendsAmmo: not false }) || (int)cardSource.Type != 1)
		{
			return 0m;
		}
		return ((RelicModel)this).DynamicVars["Amount"].BaseValue;
	}
}
