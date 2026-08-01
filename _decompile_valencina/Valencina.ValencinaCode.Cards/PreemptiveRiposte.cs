using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Cards;

public sealed class PreemptiveRiposte : ValencinaCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new DamageVar(0m, (ValueProp)8),
		(DynamicVar)new BlockVar("Amount", 5m, (ValueProp)8)
	});

	public PreemptiveRiposte()
		: base(1, (CardType)1, (CardRarity)4, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		Creature target = play.Target;
		if (target != null)
		{
			Player owner = ((CardModel)this).Owner;
			if (((owner != null) ? owner.Creature : null) != null)
			{
				await GainTemporaryDodgeThreshold(((CardModel)this).DynamicVars["Amount"], play);
				int num = ((CardModel)this).Owner.Creature.GetPower<InstantForesightPower>()?.DodgeValue ?? 0;
				await ExecuteAttackCommandAsync(choiceContext, DamageCmd.Attack((decimal)num).FromCard((CardModel)(object)this).Targeting(target)
					.WithHitFx("vfx/vfx_attack_slash", (string)null, (string)null));
			}
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).EnergyCost.UpgradeBy(-1);
	}
}
