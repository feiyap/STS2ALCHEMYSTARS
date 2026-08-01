using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Cards;

public sealed class FamilyShame : ValencinaCard, IInstantAttackCard
{
	public int InstantAmmoCost => 0;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)new DamageVar(3m, (ValueProp)8));

	public FamilyShame()
		: base(1, (CardType)1, (CardRarity)5, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		Creature target = play.Target;
		if (target != null)
		{
			int hitCount = 2 + ReadCurrentFloorNumber() / 10;
			await InstantAttackHelper.ExecuteAgainstTargetAsync(this, choiceContext, target, hitCount);
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(2m);
	}
}
