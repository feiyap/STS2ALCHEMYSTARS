using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Valencina.ValencinaCode.Cards;

public sealed class ShatteredHammer : ValencinaCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new EnergyVar(2),
		new DynamicVar("Void", 1m)
	});

	public ShatteredHammer()
		: base(0, (CardType)2, (CardRarity)3, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		await PlayerCmd.GainEnergy(((DynamicVar)((CardModel)this).DynamicVars.Energy).BaseValue, ((CardModel)this).Owner);
		await ShuffleSingleVoidIntoDrawPileAsync();
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Energy).UpgradeValueBy(1m);
	}

	private async Task ShuffleSingleVoidIntoDrawPileAsync()
	{
		if (((CardModel)this).CombatState != null && ((CardModel)this).Owner != null)
		{
			Void obj = ((CardModel)this).CombatState.CreateCard<Void>(((CardModel)this).Owner);
			PlayerCombatState playerCombatState = ((CardModel)this).Owner.PlayerCombatState;
			CardPilePosition val = (CardPilePosition)((playerCombatState != null && playerCombatState.DrawPile.Cards.Count > 0) ? 3 : 2);
			CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat((CardModel)(object)obj, (PileType)1, ((CardModel)this).Owner, val), 1.2f, (CardPreviewStyle)1);
		}
	}
}
