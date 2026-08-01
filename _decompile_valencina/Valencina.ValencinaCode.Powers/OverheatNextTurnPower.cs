using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Powers;

public sealed class OverheatNextTurnPower : ValencinaPower
{
	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		Creature owner = ((PowerModel)this).Owner;
		if (((owner != null) ? owner.Player : null) == player)
		{
			((PowerModel)this).Owner.GetPower<InstantForesightPower>()?.ForceOverheat();
			await PowerCmd.Remove((PowerModel)(object)this);
		}
	}
}
