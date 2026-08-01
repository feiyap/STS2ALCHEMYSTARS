using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Compat;

namespace Valencina.ValencinaCode.Powers;

public sealed class TargetDecisionPower : ValencinaPower, IAddDumbVariablesToPowerDescription
{
	private const int HuntingTargetStacks = 1;

	private const int DamagePercentPerStack = 25;

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)0;

	public override bool AllowNegative => false;

	public void AddDumbVariablesToPowerDescription(LocString description)
	{
		description.Add("Stacks", 1m);
		description.Add("Percent", 25m);
	}

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player.Creature != ((PowerModel)this).Owner)
		{
			return;
		}
		ICombatState combatState = ((PowerModel)this).Owner.CombatState;
		if (combatState == null)
		{
			return;
		}
		List<Creature> list = combatState.HittableEnemies.Where((Creature enemy) => enemy.IsAlive).OrderBy(StableCreatureKey).ToList();
		if (list.Count == 0)
		{
			return;
		}
		Creature chosen = player.RunState.Rng.CombatTargets.NextItem<Creature>((IEnumerable<Creature>)list) ?? list[0];
		if (!chosen.IsAlive || chosen.IsDead)
		{
			return;
		}
		try
		{
			await CompatPowerCmd.Apply<HuntingTargetPower>(choiceContext, chosen, 1m, ((PowerModel)this).Owner, (CardModel?)null, silent: true);
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[TargetDecisionPower] Failed to mark target " + chosen.Name + "; continuing turn: " + ex.Message, 1);
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
