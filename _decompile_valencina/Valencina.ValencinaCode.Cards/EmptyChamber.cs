using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Cards;

public sealed class EmptyChamber : ValencinaPlaceholderPowerCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Percent", 25m));

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

	public EmptyChamber()
		: base(1, (CardRarity)3, showInCardLibrary: false, autoAdd: false)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		await CommonActions.ApplySelf<EmptyChamberPower>(choiceContext, (CardModel)(object)this, IsCardUpgraded() ? 50m : 25m, silent: false);
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Percent"].UpgradeValueBy(25m);
	}
}
