using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Powers;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Cards;

public sealed class Discipline : ValencinaPlaceholderCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		new DynamicVar("Cost", 5m),
		new DynamicVar("Charges", 2m)
	});

	public Discipline()
		: base(0, (CardType)2, (CardRarity)2, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		Player owner = ((CardModel)this).Owner;
		if (((owner != null) ? owner.Creature : null) == null || ((CardModel)this).Owner == null)
		{
			return;
		}
		InstantForesightPower power = ((CardModel)this).Owner.Creature.GetPower<InstantForesightPower>();
		if (power != null)
		{
			int cost = ((CardModel)this).DynamicVars["Cost"].IntValue;
			if (await power.SpendPrecognitionForEffectAsync(choiceContext, cost) >= cost)
			{
				await BreathingMethodService.GainChargesAsync(((CardModel)this).Owner.Creature, ((CardModel)this).DynamicVars["Charges"].IntValue, (CardModel?)(object)this, choiceContext);
			}
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Charges"].UpgradeValueBy(1m);
	}
}
