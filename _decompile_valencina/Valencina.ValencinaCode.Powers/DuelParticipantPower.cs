using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Valencina.ValencinaCode.Compat;

namespace Valencina.ValencinaCode.Powers;

public sealed class DuelParticipantPower : ValencinaPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)2;

	protected override bool IsVisibleInternal => false;

	public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Player owner = card.Owner;
		if (((owner != null) ? owner.Creature : null) != ((PowerModel)this).Owner || (int)autoPlayType != 0)
		{
			return true;
		}
		CombatManager instance = CombatManager.Instance;
		return ((instance != null) ? instance.History.CardPlaysFinished.Count(delegate(CardPlayFinishedEntry entry)
		{
			Player owner2 = entry.CardPlay.Card.Owner;
			return ((owner2 != null) ? owner2.Creature : null) == ((PowerModel)this).Owner && ((PowerModel)this).Owner.CombatState != null && ((CombatHistoryEntry)entry).HappenedThisTurn(((PowerModel)this).Owner.CombatState);
		}) : 0) < 6;
	}

	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Owner.Side == side && participants.Contains(((PowerModel)this).Owner) && ((PowerModel)this).Owner.IsAlive)
		{
			await CompatPowerCmd.Apply<StrengthPower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), ((PowerModel)this).Owner, 3m, ((PowerModel)this).Owner, (CardModel?)null, silent: false);
		}
	}
}
