using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Cards;

public sealed class Slipstep : ValencinaCard
{
	public override bool SpendsAmmo => true;

	public override int AmmoSpendPreviewAmount => (int)((CardModel)this).DynamicVars["Amount"].BaseValue;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Amount", 1m));

	public Slipstep()
		: base(1, (CardType)2, (CardRarity)3, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		int ammoPerDraw = (int)((CardModel)this).DynamicVars["Amount"].BaseValue;
		while (PileTypeExtensions.GetPile((PileType)2, ((CardModel)this).Owner).Cards.Count < 10)
		{
			CardModel drawn = (await CardPileCmd.Draw(choiceContext, 1m, ((CardModel)this).Owner, false)).FirstOrDefault();
			if (drawn != null)
			{
				if (ammoPerDraw > 0)
				{
					await AmmoSystem.TryConsumeAsync(((CardModel)this).Owner.Creature, ammoPerDraw, (CardModel?)(object)this, grantBreathingMethod: true, choiceContext);
				}
				if ((int)drawn.Type == 1)
				{
					break;
				}
				continue;
			}
			break;
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Amount"].UpgradeValueBy(1m);
	}
}
