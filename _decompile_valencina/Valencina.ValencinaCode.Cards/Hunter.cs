using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Cards;

public sealed class Hunter : ValencinaCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new DamageVar(10m, (ValueProp)8),
		new DynamicVar("Dodge", 2m)
	});

	public Hunter()
		: base(1, (CardType)1, (CardRarity)3, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		Creature target = play.Target;
		if (target != null)
		{
			await ExecuteAttackAsync(choiceContext, play);
			await CommonActions.Apply<HunterMarkPower>(choiceContext, target, (CardModel?)(object)this, ((CardModel)this).DynamicVars["Dodge"].BaseValue, silent: false);
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Dodge"].UpgradeValueBy(1m);
	}
}
