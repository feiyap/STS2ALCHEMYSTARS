using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Cards;

public sealed class ScorchingBreakthrough : ValencinaPlaceholderCard, IBurnApplyingCard
{
	private const int Hits = 5;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		new DynamicVar("Burn", 3m),
		new DynamicVar("Hits", 5m)
	});

	public ScorchingBreakthrough()
		: base(2, (CardType)2, (CardRarity)3, (TargetType)3)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		for (int i = 0; i < 5; i++)
		{
			foreach (Creature item in EnumerateOpponents().OrderBy(ValencinaCardStableKeys.Creature))
			{
				await StatusSystem.ApplyBurnAsync(item, IsCardUpgraded() ? 4 : 3, (CardModel?)(object)this, choiceContext);
			}
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Burn"].UpgradeValueBy(1m);
	}
}
