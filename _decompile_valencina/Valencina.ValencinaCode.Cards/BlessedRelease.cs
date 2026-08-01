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

public sealed class BlessedRelease : ValencinaCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new DamageVar(5m, (ValueProp)8),
		new DynamicVar("Amount", 5m)
	});

	public BlessedRelease()
		: base(0, (CardType)1, (CardRarity)3, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		Creature target = play.Target;
		if (target != null)
		{
			await ExecuteAttackAsync(choiceContext, target, 1, "vfx/vfx_attack_slash");
			await StatusSystem.ApplyTremorAsync(target, (int)((CardModel)this).DynamicVars["Amount"].BaseValue, (CardModel?)(object)this, allowStarterRelicConversion: true, choiceContext);
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Amount"].UpgradeValueBy(5m);
	}
}
