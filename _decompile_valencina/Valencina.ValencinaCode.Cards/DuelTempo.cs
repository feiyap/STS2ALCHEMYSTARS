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

public sealed class DuelTempo : ValencinaCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		new DynamicVar("Cost", 5m),
		(DynamicVar)new BlockVar("Dodge", 7m, (ValueProp)8)
	});

	public DuelTempo()
		: base(1, (CardType)3, (CardRarity)4, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		await CommonActions.ApplySelf<DuelTempoPower>(choiceContext, (CardModel)(object)this, IsCardUpgraded() ? 10m : 7m, silent: false);
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Dodge"].UpgradeValueBy(3m);
	}
}
