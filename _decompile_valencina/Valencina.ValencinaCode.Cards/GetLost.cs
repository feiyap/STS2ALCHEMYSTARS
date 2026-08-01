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

public sealed class GetLost : ValencinaCard
{
	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[3]
	{
		(DynamicVar)new BlockVar(3m, (ValueProp)8),
		new DynamicVar("Foresight", 2m),
		(DynamicVar)new BlockVar("TotalDodge", 6m, (ValueProp)8)
	});

	public GetLost()
		: base(1, (CardType)2, (CardRarity)2, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		await GainTemporaryDodgeThreshold(((CardModel)this).DynamicVars["TotalDodge"], play);
		await CommonActions.ApplySelf<GetLostCounterPower>(choiceContext, (CardModel)(object)this, 1m, silent: false);
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Block).UpgradeValueBy(1m);
		((CardModel)this).DynamicVars["TotalDodge"].UpgradeValueBy(3m);
	}
}
