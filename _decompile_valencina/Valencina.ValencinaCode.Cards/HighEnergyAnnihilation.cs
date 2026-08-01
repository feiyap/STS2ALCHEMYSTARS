using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Cards;

public sealed class HighEnergyAnnihilation : ValencinaPlaceholderCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		new DynamicVar("Destined", 1m),
		new DynamicVar("Cards", 1m)
	});

	public override IEnumerable<CardKeyword> CanonicalKeywords
	{
		get
		{
			foreach (CardKeyword canonicalKeyword in base.CanonicalKeywords)
			{
				yield return canonicalKeyword;
			}
			yield return (CardKeyword)1;
		}
	}

	public HighEnergyAnnihilation()
		: base(1, (CardType)2, (CardRarity)3, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		await CommonActions.ApplySelf<DestinedFuturePower>(choiceContext, (CardModel)(object)this, ((CardModel)this).DynamicVars["Destined"].BaseValue, silent: false);
		int num = (IsCardUpgraded() ? 99 : ((CardModel)this).DynamicVars["Cards"].IntValue);
		await CommonActions.ApplySelf<HuntingPreparationPower>(choiceContext, (CardModel)(object)this, (decimal)num, silent: false);
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Cards"].UpgradeValueBy(98m);
	}
}
