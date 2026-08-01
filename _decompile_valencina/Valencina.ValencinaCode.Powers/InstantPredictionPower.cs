using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Valencina.ValencinaCode.Compat;

namespace Valencina.ValencinaCode.Powers;

public sealed class InstantPredictionPower : ValencinaPower
{
	private int _turnNumber;

	private int _temporaryStrengthThisTurn;

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	private int CurrentTurnCap => 2 * Math.Max(1, _turnNumber) * Math.Max(1, ((PowerModel)this).Amount);

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (((PowerModel)this).Owner != null && player.Creature == ((PowerModel)this).Owner)
		{
			await RemoveTemporaryStrengthAsync(((PowerModel)this).Owner);
			_temporaryStrengthThisTurn = 0;
			_turnNumber = Math.Max(1, _turnNumber + 1);
			((PowerModel)this).InvokeDisplayAmountChanged();
		}
	}

	public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (((PowerModel)this).Owner == null)
		{
			return;
		}
		Player owner = cardPlay.Card.Owner;
		if (((owner != null) ? owner.Creature : null) == ((PowerModel)this).Owner && (int)cardPlay.Card.Type == 1)
		{
			if (_turnNumber <= 0)
			{
				_turnNumber = 1;
			}
			int currentTurnCap = CurrentTurnCap;
			if (_temporaryStrengthThisTurn < currentTurnCap)
			{
				_temporaryStrengthThisTurn++;
				((PowerModel)this).Flash();
				await CompatPowerCmd.Apply<StrengthPower>(choiceContext, ((PowerModel)this).Owner, 1m, ((PowerModel)this).Owner, cardPlay.Card, silent: false);
				((PowerModel)this).InvokeDisplayAmountChanged();
			}
		}
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		await ClearTemporaryStrengthAtOwnerTurnEnd(side);
	}

	public override async Task AfterRemoved(Creature oldOwner)
	{
		await RemoveTemporaryStrengthAsync(oldOwner);
		_temporaryStrengthThisTurn = 0;
	}

	private async Task ClearTemporaryStrengthAtOwnerTurnEnd(CombatSide side)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Owner != null && side == ((PowerModel)this).Owner.Side)
		{
			await RemoveTemporaryStrengthAsync(((PowerModel)this).Owner);
			_temporaryStrengthThisTurn = 0;
			((PowerModel)this).InvokeDisplayAmountChanged();
		}
	}

	private async Task RemoveTemporaryStrengthAsync(Creature owner)
	{
		int temporaryStrengthThisTurn = _temporaryStrengthThisTurn;
		if (temporaryStrengthThisTurn > 0)
		{
			await CompatPowerCmd.Apply<StrengthPower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), owner, (decimal)(-temporaryStrengthThisTurn), owner, (CardModel?)null, silent: false);
		}
	}
}
