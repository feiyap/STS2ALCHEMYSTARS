using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Valencina.ValencinaCode.Cards;

public sealed class OldManVodka : ValencinaCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords
	{
		get
		{
			foreach (CardKeyword canonicalKeyword in base.CanonicalKeywords)
			{
				yield return canonicalKeyword;
			}
			yield return (CardKeyword)3;
			yield return (CardKeyword)1;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new EnergyVar(1),
		new DynamicVar("Void", 1m)
	});

	public OldManVodka()
		: base(0, (CardType)2, (CardRarity)3, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		Player owner = ((CardModel)this).Owner;
		Creature val = ((owner != null) ? owner.Creature : null);
		if (owner != null && val != null)
		{
			await PlayerCmd.GainEnergy(((DynamicVar)((CardModel)this).DynamicVars.Energy).BaseValue, owner);
			await CardPileCmd.Draw(choiceContext, 2m, owner, false);
			if (((CardModel)this).DynamicVars["Void"].BaseValue > 0m)
			{
				await ShuffleSingleVoidIntoDrawPileAsync();
			}
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Energy).UpgradeValueBy(1m);
	}

	private async Task ShuffleSingleVoidIntoDrawPileAsync()
	{
		if (((CardModel)this).CombatState != null)
		{
			CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat((CardModel)(object)((CardModel)this).CombatState.CreateCard<Void>(((CardModel)this).Owner), (PileType)1, ((CardModel)this).Owner, (CardPilePosition)(DrawPileHasCards() ? 3 : 2)), 1.2f, (CardPreviewStyle)1);
		}
	}

	private bool DrawPileHasCards()
	{
		Player owner = ((CardModel)this).Owner;
		if (owner == null)
		{
			return false;
		}
		PlayerCombatState playerCombatState = owner.PlayerCombatState;
		return ((playerCombatState != null) ? new int?(playerCombatState.DrawPile.Cards.Count) : ((int?)null)) > 0;
	}
}
