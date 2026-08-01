using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Cards;

public sealed class CrystalClear : ValencinaPlaceholderPowerCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Cards", 1m));

	public CrystalClear()
		: base(2, (CardRarity)3)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		await CommonActions.ApplySelf<CrystalClearPower>(choiceContext, (CardModel)(object)this, IsCardUpgraded() ? 2m : 1m, silent: false);
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Cards"].UpgradeValueBy(1m);
	}
}
