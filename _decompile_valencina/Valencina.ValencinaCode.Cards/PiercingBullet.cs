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
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Cards;

public sealed class PiercingBullet : ValencinaCard, IBurnApplyingCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new DamageVar(3m, (ValueProp)8),
		new DynamicVar("Amount", 3m)
	});

	public PiercingBullet()
		: base(2, (CardType)1, (CardRarity)3, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		Creature target = play.Target;
		if (target == null)
		{
			return;
		}
		int amount = (int)((CardModel)this).DynamicVars["Amount"].BaseValue;
		AttackCommand command = CommonActions.CardAttack((CardModel)(object)this, target, 3);
		await ExecuteAttackCommandAsync(choiceContext, command);
		if (!target.IsAlive)
		{
			return;
		}
		foreach (DamageResult item in CommonActions.DamageResults(command))
		{
			if (item.Receiver == target)
			{
				await StatusSystem.ApplyTremorAsync(target, amount, (CardModel?)(object)this, allowStarterRelicConversion: true, choiceContext);
				await StatusSystem.ApplyBurnAsync(target, amount, (CardModel?)(object)this, choiceContext);
			}
		}
		await StatusSystem.DetonateTremorAsync(target, (CardModel?)(object)this, consumeStacks: true, choiceContext);
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(1m);
		((CardModel)this).DynamicVars["Amount"].UpgradeValueBy(2m);
	}
}
