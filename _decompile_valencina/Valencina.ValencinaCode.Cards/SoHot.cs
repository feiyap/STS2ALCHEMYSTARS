using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Cards;

public sealed class SoHot : ValencinaPlaceholderPowerCard
{
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
			if (IsCardUpgraded() && emitted.Add((CardKeyword)3))
			{
				yield return (CardKeyword)3;
			}
		}
	}

	public SoHot()
		: base(1, (CardRarity)4)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		await CommonActions.ApplySelf<SoHotPower>(choiceContext, (CardModel)(object)this, 1m, silent: false);
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).AddKeyword((CardKeyword)3);
		TryEnableInnate();
	}
}
