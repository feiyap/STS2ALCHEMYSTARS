using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Compat;

namespace Valencina.ValencinaCode.Powers;

public sealed class UnyieldingPower : ValencinaPower, IAddDumbVariablesToPowerDescription
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public void AddDumbVariablesToPowerDescription(LocString description)
	{
		description.Add("Amount", (decimal)Math.Max(0, ((PowerModel)this).Amount));
	}

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (((PowerModel)this).Owner != null && player.Creature == ((PowerModel)this).Owner && ((PowerModel)this).Amount > 0)
		{
			((PowerModel)this).Flash();
			await CompatPowerCmd.Apply<DestinedFuturePower>(choiceContext, ((PowerModel)this).Owner, (decimal)Math.Max(0, ((PowerModel)this).Amount), ((PowerModel)this).Owner, (CardModel?)null, silent: false);
		}
	}
}
