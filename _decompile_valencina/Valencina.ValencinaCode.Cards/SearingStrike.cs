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

public sealed class SearingStrike : ValencinaPlaceholderCard, IBurnApplyingCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new DamageVar(6m, (ValueProp)8),
		new DynamicVar("Burn", 5m)
	});

	public SearingStrike()
		: base(1, (CardType)1, (CardRarity)2, (TargetType)2, showInCardLibrary: false, autoAdd: false)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		await ExecuteAttackAsync(choiceContext, play, 1, "vfx/vfx_attack_slash");
		Creature target = play.Target;
		if (target != null)
		{
			await StatusSystem.ApplyBurnAsync(target, (int)((CardModel)this).DynamicVars["Burn"].BaseValue, (CardModel?)(object)this, choiceContext);
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Burn"].UpgradeValueBy(3m);
	}
}
