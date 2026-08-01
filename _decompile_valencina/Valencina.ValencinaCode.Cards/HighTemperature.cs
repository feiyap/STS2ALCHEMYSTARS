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

public sealed class HighTemperature : ValencinaCard, IBurnApplyingCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		new DynamicVar("Burn", 4m),
		new DynamicVar("StrengthDown", 2m)
	});

	public HighTemperature()
		: base(1, (CardType)2, (CardRarity)2, (TargetType)3)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		foreach (Creature creature in EnumerateOpponents())
		{
			await StatusSystem.ApplyBurnAsync(creature, (int)((CardModel)this).DynamicVars["Burn"].BaseValue, (CardModel?)(object)this, choiceContext);
			await CommonActions.Apply<HighTemperatureStrengthDownPower>(choiceContext, creature, (CardModel?)(object)this, ((CardModel)this).DynamicVars["StrengthDown"].BaseValue, silent: false);
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Burn"].UpgradeValueBy(2m);
		((CardModel)this).DynamicVars["StrengthDown"].UpgradeValueBy(1m);
	}
}
