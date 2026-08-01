using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Powers;

public sealed class KaiserRustlePower : ValencinaPower
{
	private bool _activeAfterTurnStart;

	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)2;

	public override Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
	{
		if (player == ((PowerModel)this).Owner.Player)
		{
			_activeAfterTurnStart = true;
		}
		return Task.CompletedTask;
	}

	public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		if (!(!_activeAfterTurnStart || fromHandDraw))
		{
			Player owner = card.Owner;
			if (((owner != null) ? owner.Creature : null) == ((PowerModel)this).Owner && ((PowerModel)this).Owner.Player != null)
			{
				((PowerModel)this).Flash();
				await PlayerCmd.LoseEnergy(1m, ((PowerModel)this).Owner.Player);
			}
		}
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Owner.Side == side)
		{
			await PowerCmd.Remove((PowerModel)(object)this);
		}
	}
}
