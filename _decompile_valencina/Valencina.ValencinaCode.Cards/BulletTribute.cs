using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Cards;

public sealed class BulletTribute : ValencinaCard
{
	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)new BlockVar(2m, (ValueProp)8));

	public BulletTribute()
		: base(1, (CardType)2, (CardRarity)3, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		int num = Math.Max(0, AmmoSystem.MaxAmmoFor(((CardModel)this).Owner.Creature) - AmmoSystem.CurrentAmmo(((CardModel)this).Owner.Creature));
		if (num > 0)
		{
			int num2 = await AmmoSystem.AddAmmoAsync(((CardModel)this).Owner.Creature, num, (CardModel?)(object)this, choiceContext);
			if (num2 > 0)
			{
				await GainTemporaryDodgeThreshold(((CardModel)this).DynamicVars.Block, num2, play);
			}
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Block).UpgradeValueBy(1m);
	}
}
