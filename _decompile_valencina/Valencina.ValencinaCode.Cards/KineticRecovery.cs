using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Cards;

public sealed class KineticRecovery : ValencinaCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		new DynamicVar("Amount", 3m),
		(DynamicVar)new EnergyVar(3)
	});

	protected override bool ShouldGlowGoldInternal
	{
		get
		{
			Player owner = ((CardModel)this).Owner;
			return AmmoSystem.AmmoSpentThisTurn((owner != null) ? owner.Creature : null) >= (int)((CardModel)this).DynamicVars["Amount"].BaseValue;
		}
	}

	public KineticRecovery()
		: base(1, (CardType)2, (CardRarity)3, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		Player owner = ((CardModel)this).Owner;
		if (owner != null && AmmoSystem.AmmoSpentThisTurn(owner.Creature) >= (int)((CardModel)this).DynamicVars["Amount"].BaseValue)
		{
			await PlayerCmd.GainEnergy(((DynamicVar)((CardModel)this).DynamicVars.Energy).BaseValue, owner);
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).EnergyCost.UpgradeBy(-1);
		((DynamicVar)((CardModel)this).DynamicVars.Energy).UpgradeValueBy(-1m);
	}
}
