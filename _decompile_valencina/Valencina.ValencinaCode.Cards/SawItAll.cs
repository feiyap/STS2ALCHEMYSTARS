using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Cards;

public sealed class SawItAll : ValencinaPlaceholderCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Amount", 3m));

	public SawItAll()
		: base(2, (CardType)2, (CardRarity)4, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		Player owner = ((CardModel)this).Owner;
		if (((owner != null) ? owner.Creature : null) != null)
		{
			await CommonActions.Apply<DestinedFuturePower>(choiceContext, ((CardModel)this).Owner.Creature, (CardModel?)(object)this, ((CardModel)this).DynamicVars["Amount"].BaseValue, silent: false);
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Amount"].UpgradeValueBy(2m);
	}
}
