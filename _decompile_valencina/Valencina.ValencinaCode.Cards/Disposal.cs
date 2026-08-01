using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Cards;

public sealed class Disposal : ValencinaCard
{
	private const int DestinedFutureGain = 10;

	public override IEnumerable<CardKeyword> CanonicalKeywords
	{
		get
		{
			foreach (CardKeyword canonicalKeyword in base.CanonicalKeywords)
			{
				yield return canonicalKeyword;
			}
			yield return (CardKeyword)1;
			if (IsCardUpgraded())
			{
				yield return (CardKeyword)5;
			}
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Amount", 10m));

	public Disposal()
		: base(1, (CardType)2, (CardRarity)5, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		Player owner = ((CardModel)this).Owner;
		if (((owner != null) ? owner.Creature : null) != null)
		{
			await CompatPowerCmd.Apply<DestinedFuturePower>(choiceContext, ((CardModel)this).Owner.Creature, 10m, ((CardModel)this).Owner.Creature, (CardModel?)(object)this, silent: false);
			((CardModel)this).Owner.Creature.GetPower<DestinedFuturePower>()?.QueueWillDisposalEnhancement();
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).AddKeyword((CardKeyword)5);
		TryEnableRetain();
	}
}
