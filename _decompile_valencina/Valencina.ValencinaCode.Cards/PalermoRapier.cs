using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;
using Valencina.ValencinaCode.Systems;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Cards;

public sealed class PalermoRapier : ValencinaCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new DamageVar(3m, (ValueProp)8),
		new DynamicVar("Amount", 1m)
	});

	public PalermoRapier()
		: base(2, (CardType)1, (CardRarity)4, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		Creature target = play.Target;
		if (target != null)
		{
			BurnPower? burnPower = CreaturePowerAccess.Find<BurnPower>(target);
			int num = ((burnPower != null) ? ((PowerModel)burnPower).Amount : 0);
			int num2 = StatusSystem.TremorAmount(target);
			int num3 = (num + num2) / 10;
			int hitCount = (int)((CardModel)this).DynamicVars["Amount"].BaseValue + num3;
			decimal damage = ReadDamageValue();
			AttackCommand command = CommonActions.CardAttack((CardModel)(object)this, target, damage, hitCount, "vfx/vfx_attack_slash");
			await ExecuteAttackCommandAsync(choiceContext, command);
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(2m);
	}
}
