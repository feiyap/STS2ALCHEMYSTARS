using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Cards;

public sealed class EjectCasing : ValencinaPlaceholderCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		new DynamicVar("Ammo", 2m),
		(DynamicVar)new CardsVar(1)
	});

	public EjectCasing()
		: base(0, (CardType)2, (CardRarity)3, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		Player owner = ((CardModel)this).Owner;
		if (((owner != null) ? owner.Creature : null) != null && ((CardModel)this).Owner != null)
		{
			await AmmoSystem.AddAmmoAsync(((CardModel)this).Owner.Creature, (int)((CardModel)this).DynamicVars["Ammo"].BaseValue, (CardModel?)(object)this, choiceContext);
			await CardPileCmd.Draw(choiceContext, ((DynamicVar)((CardModel)this).DynamicVars.Cards).BaseValue, ((CardModel)this).Owner, false);
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Ammo"].UpgradeValueBy(1m);
	}
}
