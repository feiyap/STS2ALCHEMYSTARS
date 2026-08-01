using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Precognition;

namespace Valencina.ValencinaCode.Powers;

public sealed class TightBitePower : ValencinaPower
{
	private bool _triggeredThisTurn;

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		Creature owner = ((PowerModel)this).Owner;
		if (((owner != null) ? owner.Player : null) == player)
		{
			_triggeredThisTurn = false;
		}
		return Task.CompletedTask;
	}

	public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (((PowerModel)this).Owner == null)
		{
			return;
		}
		Player owner = cardPlay.Card.Owner;
		if (((owner != null) ? owner.Creature : null) != ((PowerModel)this).Owner || (int)cardPlay.Card.Type != 1 || cardPlay.Card is IPrecognitionVirtualCounterCard || _triggeredThisTurn)
		{
			return;
		}
		InstantForesightPower precognition = ((PowerModel)this).Owner.GetPower<InstantForesightPower>();
		if (precognition == null)
		{
			return;
		}
		ICombatState combatState = ((PowerModel)this).Owner.CombatState;
		List<Creature> list = ((combatState != null) ? combatState.HittableEnemies.Where((Creature enemy) => enemy.IsAlive).OrderBy(ValencinaPowerStableKeys.Creature).ToList() : null) ?? new List<Creature>();
		if (list.Count == 0)
		{
			return;
		}
		_triggeredThisTurn = true;
		((PowerModel)this).Flash();
		foreach (Creature item in list)
		{
			await precognition.TriggerCounterAgainstAsync(choiceContext, item);
		}
	}

	public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return ResetTriggerAtOwnerTurnEnd(side);
	}

	private Task ResetTriggerAtOwnerTurnEnd(CombatSide side)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Owner != null && side == ((PowerModel)this).Owner.Side)
		{
			_triggeredThisTurn = false;
		}
		return Task.CompletedTask;
	}
}
