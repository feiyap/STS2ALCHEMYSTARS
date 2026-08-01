using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Cards;

public sealed class ContinuousCutting : ValencinaPlaceholderCard
{
	protected override bool HasEnergyCostX => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		new DynamicVar("Bonus", 1m),
		(DynamicVar)new StringVar("Times", "X+1")
	});

	public ContinuousCutting()
		: base(0, (CardType)1, (CardRarity)4, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		InstantForesightPower power = ((CardModel)this).Owner.Creature.GetPower<InstantForesightPower>();
		if (power != null)
		{
			Creature target = play.Target;
			Creature val = (Creature)((target != null && target.IsAlive) ? ((object)play.Target) : ((object)(from enemy in EnumerateOpponents()
				where enemy.IsAlive
				select enemy).OrderBy(ValencinaCardStableKeys.Creature).FirstOrDefault()));
			if (val != null)
			{
				int times = ((CardModel)this).ResolveEnergyXValue() + ((!IsCardUpgraded()) ? 1 : 2);
				await power.TriggerCounterAgainstImmediatelyAsync(choiceContext, val, times);
			}
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Bonus"].UpgradeValueBy(1m);
		DynamicVar obj = ((CardModel)this).DynamicVars["Times"];
		StringVar val = (StringVar)(object)((obj is StringVar) ? obj : null);
		if (val != null)
		{
			val.StringValue = "X+2";
		}
	}
}
