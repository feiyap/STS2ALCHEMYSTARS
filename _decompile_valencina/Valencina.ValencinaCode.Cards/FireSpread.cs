using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Cards;

public sealed class FireSpread : ValencinaPlaceholderPowerCard
{
	public FireSpread()
		: base(1, (CardRarity)4)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		await CommonActions.ApplySelf<TargetDecisionPower>(choiceContext, (CardModel)(object)this, 1m, silent: false);
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).AddKeyword((CardKeyword)3);
		TryEnableInnate();
	}
}
