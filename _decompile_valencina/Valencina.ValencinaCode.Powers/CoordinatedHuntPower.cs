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

namespace Valencina.ValencinaCode.Powers;

public sealed class CoordinatedHuntPower : ValencinaPower
{
	public const int EnergyThreshold = 2;

	private bool _triggeringCounter;

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)0;

	public override bool AllowNegative => false;

	public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (_triggeringCounter || ((PowerModel)this).Owner == null)
		{
			return;
		}
		Player owner = cardPlay.Card.Owner;
		Creature val = ((owner != null) ? owner.Creature : null);
		if (val == null || val == ((PowerModel)this).Owner || val.Player == null || val.Side != ((PowerModel)this).Owner.Side)
		{
			return;
		}
		ResourceInfo resources = cardPlay.Resources;
		if (((ResourceInfo)(ref resources)).EnergyValue <= 2)
		{
			return;
		}
		InstantForesightPower power = ((PowerModel)this).Owner.GetPower<InstantForesightPower>();
		if (power == null)
		{
			return;
		}
		ICombatState combatState = ((PowerModel)this).Owner.CombatState;
		List<Creature> list = ((combatState != null) ? combatState.HittableEnemies.Where((Creature enemy) => enemy.IsAlive).OrderBy(StableCreatureKey).ToList() : null) ?? new List<Creature>();
		if (list.Count == 0)
		{
			return;
		}
		Player player = ((PowerModel)this).Owner.Player;
		Creature target = ((player != null) ? player.RunState.Rng.CombatTargets.NextItem<Creature>((IEnumerable<Creature>)list) : null) ?? list[0];
		((PowerModel)this).Flash();
		_triggeringCounter = true;
		try
		{
			await power.TriggerCounterAgainstAsync(choiceContext, target);
		}
		finally
		{
			_triggeringCounter = false;
		}
	}

	private static string StableCreatureKey(Creature creature)
	{
		object obj = creature.CombatId?.ToString("D10");
		if (obj == null)
		{
			Player player = creature.Player;
			obj = ((player != null) ? player.NetId.ToString() : null);
			if (obj == null)
			{
				MonsterModel monster = creature.Monster;
				obj = ((monster != null) ? ((AbstractModel)monster).Id.Entry : null) ?? creature.Name;
			}
		}
		return (string)obj;
	}
}
