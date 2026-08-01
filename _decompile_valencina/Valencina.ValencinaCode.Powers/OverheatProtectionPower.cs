using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Valencina.ValencinaCode.Powers;

public sealed class OverheatProtectionPower : ValencinaPower, IAddDumbVariablesToPowerDescription
{
	private const int Threshold = 15;

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public void AddDumbVariablesToPowerDescription(LocString description)
	{
		description.Add("Threshold", 15m);
		description.Add("Block", (decimal)Math.Max(0, ((PowerModel)this).Amount));
	}

	public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
	{
		if (((PowerModel)this).Owner != null && player.Creature == ((PowerModel)this).Owner && ((PowerModel)this).Amount > 0)
		{
			InstantForesightPower power = ((PowerModel)this).Owner.GetPower<InstantForesightPower>();
			if (power == null || power.PrecognitionSpentLastTurn > 15)
			{
				((PowerModel)this).Flash();
				await CreatureCmd.GainBlock(((PowerModel)this).Owner, (decimal)((PowerModel)this).Amount, (ValueProp)4, (CardPlay)null, false);
			}
		}
	}
}
