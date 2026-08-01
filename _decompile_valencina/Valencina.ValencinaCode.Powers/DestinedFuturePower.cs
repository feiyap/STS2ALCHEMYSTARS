using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Cards;
using Valencina.ValencinaCode.Compat;

namespace Valencina.ValencinaCode.Powers;

public sealed class DestinedFuturePower : ValencinaPower, IAddDumbVariablesToPowerDescription
{
	private bool _willEnhancementThisTurn;

	private bool _isResolving;

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public void AddDumbVariablesToPowerDescription(LocString description)
	{
		int num = Math.Max(0, ((PowerModel)this).Amount);
		description.Add("Current", (decimal)num);
		description.Add("Threshold", 1m);
		description.Add("ExtraHits", (decimal)num);
		description.Add("ExtraDetonations", (decimal)num);
	}

	public void QueueWillDisposalEnhancement()
	{
		_willEnhancementThisTurn = true;
	}

	public Task ValencinaAfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		return Task.CompletedTask;
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		await ResolveAtTurnEnd(choiceContext, side);
	}

	private async Task ResolveAtTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (_isResolving || ((PowerModel)this).Owner == null || side != ((PowerModel)this).Owner.Side)
		{
			return;
		}
		_isResolving = true;
		int num = Math.Max(0, ((PowerModel)this).Amount);
		try
		{
			if (num > 0)
			{
				((PowerModel)this).Flash();
				DisposalGenerationEnhancement enhancement = (_willEnhancementThisTurn ? DisposalGenerationEnhancement.Will : DisposalGenerationEnhancement.None);
				await CompatPowerCmd.Apply<PendingDisposalPower>(choiceContext, ((PowerModel)this).Owner, PendingDisposalPower.Encode(num, enhancement), ((PowerModel)this).Owner, (CardModel?)null, silent: false);
				_willEnhancementThisTurn = false;
				((PowerModel)this).SetAmount(0, false);
			}
		}
		finally
		{
			_isResolving = false;
		}
	}
}
