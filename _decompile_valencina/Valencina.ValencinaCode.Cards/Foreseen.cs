using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Cards;

public sealed class Foreseen : ValencinaCard
{
	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new BlockVar(12m, (ValueProp)8),
		new DynamicVar("Destined", 1m)
	});

	public Foreseen()
		: base(2, (CardType)2, (CardRarity)3, (TargetType)1, showInCardLibrary: false, autoAdd: false)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		await GainTemporaryDodgeThreshold(((CardModel)this).DynamicVars.Block, play);
		await CommonActions.ApplySelf<DestinedFuturePower>(choiceContext, (CardModel)(object)this, ((CardModel)this).DynamicVars["Destined"].BaseValue, silent: false);
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Block).UpgradeValueBy(4m);
	}
}
