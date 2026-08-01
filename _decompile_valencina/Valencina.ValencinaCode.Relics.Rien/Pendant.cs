using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Relics.Rien;

public sealed class Pendant : RienRelic
{
	private const string AmountKey = "Amount";

	protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[1] { (DynamicVar)new PowerVar<BreathingMethodPower>("Amount", 3m) };

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player != ((RelicModel)this).Owner)
		{
			return;
		}
		Player owner = ((RelicModel)this).Owner;
		object obj;
		if (owner == null)
		{
			obj = null;
		}
		else
		{
			Creature creature = owner.Creature;
			obj = ((creature != null) ? creature.CombatState : null);
		}
		if (obj == null)
		{
			return;
		}
		((RelicModel)this).Flash();
		foreach (Player player2 in ((IPlayerCollection)((RelicModel)this).Owner.RunState).Players)
		{
			Creature creature2 = player2.Creature;
			if (creature2 != null && creature2.IsAlive)
			{
				await CompatPowerCmd.Apply<BreathingMethodPower>(choiceContext, creature2, ((RelicModel)this).DynamicVars["Amount"].BaseValue, ((RelicModel)this).Owner.Creature, (CardModel?)null, silent: false);
			}
		}
	}
}
