using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Relics.Rien;

public sealed class SomeonesWork : RienRelic
{
	private const string AmountKey = "Amount";

	protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[1] { (DynamicVar)new PowerVar<BreathingMethodPower>("Amount", 1m) };

	public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
	{
		Player owner = ((RelicModel)this).Owner;
		if (((owner != null) ? owner.Creature : null) != null && dealer == ((RelicModel)this).Owner.Creature)
		{
			if (cardSource != null && (int)cardSource.Type == 1 && target.Side != ((RelicModel)this).Owner.Creature.Side && result.TotalDamage > 0 && CreaturePowerAccess.Enumerate(target).Any((PowerModel power) => (int)power.Type == 2))
			{
				((RelicModel)this).Flash();
				await CompatPowerCmd.Apply<BreathingMethodPower>(choiceContext, ((RelicModel)this).Owner.Creature, ((RelicModel)this).DynamicVars["Amount"].BaseValue, ((RelicModel)this).Owner.Creature, (CardModel?)null, silent: false);
			}
		}
	}
}
