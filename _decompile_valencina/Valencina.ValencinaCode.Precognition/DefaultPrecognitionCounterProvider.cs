using System;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Precognition;

public sealed class DefaultPrecognitionCounterProvider : IPrecognitionCounterCardProvider
{
	public int Priority => 0;

	public bool CanProvide(PrecognitionCounterContext context)
	{
		Creature creature = context.Owner.Creature;
		if (creature != null && creature.IsAlive)
		{
			return context.Attacker.IsAlive;
		}
		return false;
	}

	public CardModel CreateCounterCard(PrecognitionCounterContext context)
	{
		return (CardModel)(object)((context.Owner.Creature ?? throw new InvalidOperationException("Precognition counter owner has no combat creature.")).CombatState ?? throw new InvalidOperationException("Precognition counter owner has no combat state.")).CreateCard<PrecognitionJieTuCounterCard>(context.Owner);
	}
}
