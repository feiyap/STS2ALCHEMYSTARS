using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Cards;

public sealed class Maim : ValencinaCard
{
	public override bool SpendsAmmo => true;

	public override int AmmoSpendPreviewAmount => (int)((CardModel)this).DynamicVars["Amount"].BaseValue;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new DamageVar(6m, (ValueProp)8),
		new DynamicVar("Amount", 1m)
	});

	public Maim()
		: base(1, (CardType)1, (CardRarity)2, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		Creature target = play.Target;
		if (target != null)
		{
			await ExecuteAttackAsync(choiceContext, play);
			await CommonActions.Apply<VulnerablePower>(choiceContext, target, (CardModel?)(object)this, ((CardModel)this).DynamicVars["Amount"].BaseValue, silent: false);
			await AmmoSystem.TryConsumeAsync(((CardModel)this).Owner.Creature, (int)((CardModel)this).DynamicVars["Amount"].BaseValue, (CardModel?)(object)this, grantBreathingMethod: true, choiceContext);
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Amount"].UpgradeValueBy(1m);
	}
}
