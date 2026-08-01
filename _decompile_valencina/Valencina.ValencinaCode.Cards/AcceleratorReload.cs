using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Systems;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Cards;

public sealed class AcceleratorReload : ValencinaCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Amount", 10m));

	public AcceleratorReload()
		: base(1, (CardType)2, (CardRarity)3, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		int amount = (int)((CardModel)this).DynamicVars["Amount"].BaseValue;
		int before = AmmoSystem.CurrentAmmo(((CardModel)this).Owner.Creature);
		await AmmoSystem.AddAmmoAsync(((CardModel)this).Owner.Creature, amount, (CardModel?)(object)this, choiceContext);
		int num = Math.Max(0, before + amount - AmmoSystem.MaxAmmoFor(((CardModel)this).Owner.Creature)) / 3;
		if (num > 0)
		{
			await BreathingMethodService.GainChargesAsync(((CardModel)this).Owner.Creature, num, (CardModel?)(object)this, choiceContext);
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Amount"].UpgradeValueBy(4m);
	}
}
