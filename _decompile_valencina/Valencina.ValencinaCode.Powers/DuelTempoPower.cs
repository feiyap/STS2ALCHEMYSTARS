using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Powers;

public sealed class DuelTempoPower : ValencinaPower
{
	private const int PrecognitionCost = 5;

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (((PowerModel)this).Owner == null || player.Creature != ((PowerModel)this).Owner || ((PowerModel)this).Amount <= 0)
		{
			return;
		}
		InstantForesightPower power = ((PowerModel)this).Owner.GetPower<InstantForesightPower>();
		if (power != null && await power.SpendPrecognitionForEffectAsync(choiceContext, 5) >= 5)
		{
			int num = Math.Max(0, ((PowerModel)this).Amount);
			if (num > 0)
			{
				((PowerModel)this).Owner.GetPower<InstantForesightPower>()?.GainTemporaryDodgeThreshold(num);
				((PowerModel)this).Flash();
			}
		}
	}
}
