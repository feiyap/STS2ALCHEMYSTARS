using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Cards;

public sealed class LieInWait : ValencinaPlaceholderPowerCard
{
	protected override IEnumerable<CardKeyword> TooltipKeywords
	{
		get
		{
			yield return (CardKeyword)2;
			yield return (CardKeyword)5;
		}
	}

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

	public LieInWait()
		: base(1, (CardRarity)3)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		await CommonActions.ApplySelf<LieInWaitPower>(choiceContext, (CardModel)(object)this, 1m, silent: false);
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).AddKeyword((CardKeyword)3);
		TryEnableInnate();
	}
}
