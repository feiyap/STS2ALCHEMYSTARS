using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Relics.Rien;

public sealed class Reverberation : RienRelic
{
	private const int TremorThreshold = 15;

	protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[3]
	{
		new DynamicVar("Tremor", 15m),
		(DynamicVar)new PowerVar<WeakPower>("Weak", 1m),
		(DynamicVar)new PowerVar<VulnerablePower>("Vulnerable", 1m)
	};

	public async Task ValencinaAfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		Player owner = ((RelicModel)this).Owner;
		if (((owner != null) ? owner.Creature : null) == null || ((RelicModel)this).Owner.Creature.CombatState == null || side == ((RelicModel)this).Owner.Creature.Side)
		{
			return;
		}
		bool flashed = false;
		foreach (Creature enemy in participants.ToList())
		{
			if (enemy != null && enemy.IsAlive && enemy.Side != ((RelicModel)this).Owner.Creature.Side && StatusSystem.TremorAmount(enemy) > 15)
			{
				if (!flashed)
				{
					((RelicModel)this).Flash();
					flashed = true;
				}
				await CompatPowerCmd.Apply<WeakPower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), enemy, 1m, ((RelicModel)this).Owner.Creature, (CardModel?)null, silent: false);
				await CompatPowerCmd.Apply<VulnerablePower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), enemy, 1m, ((RelicModel)this).Owner.Creature, (CardModel?)null, silent: false);
			}
		}
	}
}
