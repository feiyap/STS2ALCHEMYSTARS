using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Cards;

public sealed class DamnMaggot : ValencinaPlaceholderCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		new DynamicVar("Tremor", 8m),
		new DynamicVar("StrengthDown", 2m)
	});

	public DamnMaggot()
		: base(0, (CardType)2, (CardRarity)2, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		Creature target = play.Target;
		if (target != null)
		{
			await StatusSystem.ApplyTremorAsync(target, ((CardModel)this).DynamicVars["Tremor"].IntValue, (CardModel?)(object)this, allowStarterRelicConversion: true, choiceContext);
			await CommonActions.Apply<HighTemperatureStrengthDownPower>(choiceContext, target, (CardModel?)(object)this, ((CardModel)this).DynamicVars["StrengthDown"].BaseValue, silent: false);
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Tremor"].UpgradeValueBy(2m);
		((CardModel)this).DynamicVars["StrengthDown"].UpgradeValueBy(1m);
	}
}
