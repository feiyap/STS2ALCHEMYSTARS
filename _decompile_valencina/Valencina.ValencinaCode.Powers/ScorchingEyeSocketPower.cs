using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Powers;

public sealed class ScorchingEyeSocketPower : ValencinaPower, IAddDumbVariablesToPowerDescription
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public void AddDumbVariablesToPowerDescription(LocString description)
	{
		description.Add("Loss", (decimal)Math.Max(0, ((PowerModel)this).Amount));
	}

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (((PowerModel)this).Owner != null && player.Creature == ((PowerModel)this).Owner && ((PowerModel)this).Amount > 0)
		{
			((PowerModel)this).Flash();
			InstantForesightPower power = ((PowerModel)this).Owner.GetPower<InstantForesightPower>();
			if (power != null)
			{
				await power.SpendPrecognitionForEffectAsync(choiceContext, (int)Math.Ceiling((decimal)((PowerModel)this).Amount));
			}
		}
	}
}
