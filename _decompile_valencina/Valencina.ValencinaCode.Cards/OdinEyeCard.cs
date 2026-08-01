using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Cards;

public sealed class OdinEyeCard : ValencinaPlaceholderPowerCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[3]
	{
		new DynamicVar("OldRatio", 6m),
		new DynamicVar("NewRatio", 3m),
		new DynamicVar("Amount", 10m)
	});

	public OdinEyeCard()
		: base(3, (CardRarity)4)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		await CommonActions.ApplySelf<OdinEyeRatioPower>(choiceContext, (CardModel)(object)this, 1m, silent: false);
		await CommonActions.ApplySelf<MemoryExpansionPower>(choiceContext, (CardModel)(object)this, ((CardModel)this).DynamicVars["Amount"].BaseValue, silent: false);
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).EnergyCost.UpgradeBy(-1);
	}
}
