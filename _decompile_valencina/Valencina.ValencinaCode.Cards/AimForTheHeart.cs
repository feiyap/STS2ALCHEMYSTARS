using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Cards;

public sealed class AimForTheHeart : ValencinaPlaceholderCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("All", 0m));

	public AimForTheHeart()
		: base(0, (CardType)2, (CardRarity)3, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
	{
		Player owner = ((CardModel)this).Owner;
		InstantForesightPower power = ((owner != null) ? owner.Creature.GetPower<InstantForesightPower>() : null);
		if (power == null)
		{
			return;
		}
		List<Creature> list = (from enemy in EnumerateOpponents()
			where enemy.IsAlive
			select enemy).OrderBy(StableCreatureKey).ToList();
		if (list.Count == 0)
		{
			return;
		}
		if (IsCardUpgraded())
		{
			foreach (Creature item in list)
			{
				await power.TriggerCounterAgainstAsync(choiceContext, item);
			}
			return;
		}
		Player owner2 = ((CardModel)this).Owner;
		object obj;
		if (owner2 == null)
		{
			obj = null;
		}
		else
		{
			IRunState runState = owner2.RunState;
			if (runState == null)
			{
				obj = null;
			}
			else
			{
				RunRngSet rng = runState.Rng;
				if (rng == null)
				{
					obj = null;
				}
				else
				{
					Rng combatTargets = rng.CombatTargets;
					obj = ((combatTargets != null) ? combatTargets.NextItem<Creature>((IEnumerable<Creature>)list) : null);
				}
			}
		}
		if (obj == null)
		{
			obj = list[0];
		}
		Creature target = (Creature)obj;
		await power.TriggerCounterAgainstAsync(choiceContext, target);
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["All"].UpgradeValueBy(1m);
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
