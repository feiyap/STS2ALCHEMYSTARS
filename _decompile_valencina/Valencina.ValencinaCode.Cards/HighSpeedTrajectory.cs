using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Cards;

public sealed class HighSpeedTrajectory : ValencinaCard
{
	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new BlockVar(3m, (ValueProp)8),
		new DynamicVar("Ammo", 3m)
	});

	public override IEnumerable<CardKeyword> CanonicalKeywords
	{
		get
		{
			foreach (CardKeyword canonicalKeyword in base.CanonicalKeywords)
			{
				yield return canonicalKeyword;
			}
			if (!IsCardUpgraded())
			{
				yield return (CardKeyword)1;
			}
		}
	}

	public HighSpeedTrajectory()
		: base(1, (CardType)2, (CardRarity)2, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		await GainTemporaryDodgeThreshold(((CardModel)this).DynamicVars.Block, play);
		await AmmoSystem.AddAmmoAsync(((CardModel)this).Owner.Creature, (int)((CardModel)this).DynamicVars["Ammo"].BaseValue, (CardModel?)(object)this, choiceContext);
		CardPile pile = PileTypeExtensions.GetPile((PileType)3, ((CardModel)this).Owner);
		if (pile.Cards.Count != 0)
		{
			CardSelectorPrefs val = new CardSelectorPrefs(new LocString("cards", ((AbstractModel)this).Id.Entry + ".selectionScreenPrompt"), 1);
			((CardSelectorPrefs)(ref val)).set_RequireManualConfirmation(true);
			CardSelectorPrefs val2 = val;
			CardModel val3 = (await CardSelectCmd.FromSimpleGrid(choiceContext, pile.Cards, ((CardModel)this).Owner, val2)).FirstOrDefault();
			if (val3 != null)
			{
				await CardPileCmd.Add(val3, (PileType)2, (CardPilePosition)1, (AbstractModel)null, false);
			}
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Block).UpgradeValueBy(1m);
		((CardModel)this).RemoveKeyword((CardKeyword)1);
	}
}
