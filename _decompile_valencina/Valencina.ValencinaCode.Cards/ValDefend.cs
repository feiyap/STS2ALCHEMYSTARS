using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Valencina.ValencinaCode.Cards;

public sealed class ValDefend : ValencinaCard
{
	public override bool GainsBlock => true;

	public override bool IsBasicStrikeOrDefend => true;

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)2 };

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)new BlockVar(5m, (ValueProp)8));

	public ValDefend()
		: base(1, (CardType)2, (CardRarity)1, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		await GainTemporaryDodgeThreshold(((CardModel)this).DynamicVars.Block, play);
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Block).UpgradeValueBy(2m);
	}
}
