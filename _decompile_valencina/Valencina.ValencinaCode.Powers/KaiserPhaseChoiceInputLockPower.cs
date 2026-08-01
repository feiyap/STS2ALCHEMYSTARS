using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Monsters;

namespace Valencina.ValencinaCode.Powers;

public sealed class KaiserPhaseChoiceInputLockPower : ValencinaPower
{
	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)2;

	protected override bool IsVisibleInternal => false;

	public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
	{
		Player owner = card.Owner;
		if (((owner != null) ? owner.Creature : null) == ((PowerModel)this).Owner)
		{
			return !UngezieferKaiser.HasActivePhaseTransitionChoice(((PowerModel)this).Owner.CombatState);
		}
		return true;
	}
}
