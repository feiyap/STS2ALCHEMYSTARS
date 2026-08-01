using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Cards;

public sealed class ClosingIn : ValencinaCard
{
	private int _attackCardsPlayedWhileInDiscard;

	public override bool SpendsAmmo => true;

	public override int AmmoSpendPreviewAmount => 1;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)new CardsVar(1));

	public override IEnumerable<CardKeyword> CanonicalKeywords
	{
		get
		{
			foreach (CardKeyword canonicalKeyword in base.CanonicalKeywords)
			{
				yield return canonicalKeyword;
			}
			yield return (CardKeyword)5;
		}
	}

	public ClosingIn()
		: base(0, (CardType)2, (CardRarity)4, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		_attackCardsPlayedWhileInDiscard = 0;
		await AmmoSystem.TryConsumeAsync(((CardModel)this).Owner.Creature, AmmoSpendPreviewAmount, (CardModel?)(object)this, grantBreathingMethod: true, choiceContext);
		await CardPileCmd.Draw(choiceContext, ((DynamicVar)((CardModel)this).DynamicVars.Cards).BaseValue, ((CardModel)this).Owner, false);
	}

	public async Task ValencinaAfterAttackCardPlayed(PlayerChoiceContext choiceContext)
	{
		CardPile pile = ((CardModel)this).Pile;
		if (pile == null || (int)pile.Type != 3 || ((CardModel)this).Owner == null)
		{
			return;
		}
		_attackCardsPlayedWhileInDiscard++;
		if (_attackCardsPlayedWhileInDiscard >= 3)
		{
			_attackCardsPlayedWhileInDiscard = 0;
			if (PileTypeExtensions.GetPile((PileType)2, ((CardModel)this).Owner).Cards.Count < 10)
			{
				await CardPileCmd.Add((CardModel)(object)this, (PileType)2, (CardPilePosition)1, (AbstractModel)null, false);
			}
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Cards).UpgradeValueBy(1m);
	}
}
