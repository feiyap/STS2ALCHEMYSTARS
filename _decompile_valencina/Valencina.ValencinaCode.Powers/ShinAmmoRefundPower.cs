using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Systems;
using Valencina.ValencinaCode.Vfx;

namespace Valencina.ValencinaCode.Powers;

public sealed class ShinAmmoRefundPower : ValencinaPower, IAmmoConsumedListener
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		if (((PowerModel)this).Owner != null)
		{
			ShinAuraController.Show(((PowerModel)this).Owner);
		}
		await Task.CompletedTask;
	}

	public override async Task AfterRemoved(Creature oldOwner)
	{
		ShinAuraController.Refresh(oldOwner);
		await Task.CompletedTask;
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Owner != null && side == ((PowerModel)this).Owner.Side)
		{
			await PowerCmd.TickDownDuration((PowerModel)(object)this);
		}
	}

	public async Task OnAmmoConsumedAsync(int consumed, int requested, Creature owner, Player? player, CardModel? sourceCard)
	{
		if (((PowerModel)this).Owner != null && owner == ((PowerModel)this).Owner && consumed > 0 && ((PowerModel)this).Amount > 0)
		{
			((PowerModel)this).Flash();
			ShinAuraController.Show(((PowerModel)this).Owner);
			await AmmoSystem.AddAmmoAsync(((PowerModel)this).Owner, consumed, sourceCard);
		}
	}
}
