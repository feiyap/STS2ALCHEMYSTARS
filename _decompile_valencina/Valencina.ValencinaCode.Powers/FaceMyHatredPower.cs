using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Powers;

public sealed class FaceMyHatredPower : ValencinaPower, IAddDumbVariablesToPowerDescription
{
	private CardModel? _sourceCard;

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public void AddDumbVariablesToPowerDescription(LocString description)
	{
		description.Add("Burn", (decimal)Math.Max(0, ((PowerModel)this).Amount));
	}

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		_sourceCard = cardSource;
		return Task.CompletedTask;
	}

	public Task ValencinaAfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		return Task.CompletedTask;
	}

	public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		Creature owner = ((PowerModel)this).Owner;
		if (((owner != null) ? owner.Player : null) == null || card.Owner != ((PowerModel)this).Owner.Player || ((PowerModel)this).Amount <= 0)
		{
			return;
		}
		((PowerModel)this).Flash();
		ICombatState combatState = ((PowerModel)this).Owner.CombatState;
		foreach (Creature item in ((combatState != null) ? combatState.HittableEnemies.Where((Creature enemy) => enemy.IsAlive).OrderBy(ValencinaPowerStableKeys.Creature).ToList() : null) ?? new List<Creature>())
		{
			await StatusSystem.ApplyBurnAsync(item, Math.Max(0, ((PowerModel)this).Amount), _sourceCard, choiceContext);
		}
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Owner != null && side == ((PowerModel)this).Owner.Side)
		{
			await PowerCmd.Remove((PowerModel)(object)this);
		}
	}
}
