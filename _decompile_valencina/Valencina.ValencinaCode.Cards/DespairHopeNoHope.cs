using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Cards;

public sealed class DespairHopeNoHope : ValencinaPlaceholderPowerCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		new DynamicVar("Percent", 50m),
		new DynamicVar("Cost", 2m)
	});

	public override IEnumerable<CardKeyword> CanonicalKeywords
	{
		get
		{
			HashSet<CardKeyword> emitted = new HashSet<CardKeyword>();
			foreach (CardKeyword canonicalKeyword in base.CanonicalKeywords)
			{
				if (emitted.Add(canonicalKeyword))
				{
					yield return canonicalKeyword;
				}
			}
			if (emitted.Add((CardKeyword)2))
			{
				yield return (CardKeyword)2;
			}
		}
	}

	public DespairHopeNoHope()
		: base(1, (CardRarity)4)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		await CommonActions.ApplySelf<DespairHopeNoHopePower>(choiceContext, (CardModel)(object)this, ((CardModel)this).DynamicVars["Percent"].BaseValue, silent: false);
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).EnergyCost.UpgradeBy(-1);
	}
}
