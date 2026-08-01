using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Cards;

public sealed class EnduredHumiliation : ValencinaCard
{
	public override bool SpendsAmmo => true;

	public override int AmmoSpendPreviewAmount => 2;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new DamageVar(4m, (ValueProp)8),
		new DynamicVar("Hits", 2m)
	});

	public EnduredHumiliation()
		: base(1, (CardType)1, (CardRarity)1, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		Player owner = ((CardModel)this).Owner;
		if (((owner != null) ? owner.Creature : null) != null)
		{
			Creature target = play.Target;
			if (target != null)
			{
				await ExecuteAttackAsync(choiceContext, target, (int)((CardModel)this).DynamicVars["Hits"].BaseValue, "vfx/vfx_attack_slash");
				await AmmoSystem.TryConsumeAsync(((CardModel)this).Owner.Creature, AmmoSpendPreviewAmount, (CardModel?)(object)this, grantBreathingMethod: true, choiceContext);
			}
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(2m);
	}
}
