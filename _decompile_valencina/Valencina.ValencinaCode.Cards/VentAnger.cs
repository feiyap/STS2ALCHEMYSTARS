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

public sealed class VentAnger : ValencinaPlaceholderCard, IInstantAttackCard
{
	public int InstantAmmoCost
	{
		get
		{
			if (!IsCardUpgraded())
			{
				return 1;
			}
			return 2;
		}
	}

	public override bool SpendsAmmo => true;

	public override int AmmoSpendPreviewAmount => InstantAmmoCost;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new DamageVar(4m, (ValueProp)8),
		new DynamicVar("Hits", 3m)
	});

	public VentAnger()
		: base(2, (CardType)1, (CardRarity)2, (TargetType)2, showInCardLibrary: false, autoAdd: false)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		Creature target = play.Target;
		if (target != null)
		{
			await InstantAttackHelper.ExecuteAgainstTargetAsync(this, choiceContext, target, (int)((CardModel)this).DynamicVars["Hits"].BaseValue);
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(1m);
	}
}
