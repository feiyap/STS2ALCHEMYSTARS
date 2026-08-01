using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Cards;

public sealed class Dismember : ValencinaCard
{
	public override bool SpendsAmmo => true;

	public override int AmmoSpendPreviewAmount => 1;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new DamageVar(6m, (ValueProp)8),
		new DynamicVar("Hits", 2m)
	});

	public Dismember()
		: base(2, (CardType)1, (CardRarity)3, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		Creature target = play.Target;
		if (target != null)
		{
			int hpBefore = target.CurrentHp;
			await ExecuteAttackAsync(choiceContext, target, (int)((CardModel)this).DynamicVars["Hits"].BaseValue, "vfx/vfx_attack_slash");
			await AmmoSystem.TryConsumeAsync(((CardModel)this).Owner.Creature, AmmoSpendPreviewAmount, (CardModel?)(object)this, grantBreathingMethod: true, choiceContext);
			int num = Math.Max(0, hpBefore - target.CurrentHp);
			if (num > 0)
			{
				await StatusSystem.ApplyTremorAsync(target, num, (CardModel?)(object)this, allowStarterRelicConversion: true, choiceContext);
			}
			await StatusSystem.TryConvertTremorToBurningAsync(target, (CardModel?)(object)this, choiceContext);
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Hits"].UpgradeValueBy(1m);
	}
}
