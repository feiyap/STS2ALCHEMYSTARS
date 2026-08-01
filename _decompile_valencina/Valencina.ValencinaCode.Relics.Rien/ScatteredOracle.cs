using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Relics.Rien;

public sealed class ScatteredOracle : RienRelic
{
	private const string AmountKey = "Amount";

	protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[1] { (DynamicVar)new PowerVar<DestinedFuturePower>("Amount", 2m) };

	public override async Task BeforeCombatStart()
	{
		Player owner = ((RelicModel)this).Owner;
		if (((owner != null) ? owner.Creature : null) != null)
		{
			((RelicModel)this).Flash();
			await CompatPowerCmd.Apply<DestinedFuturePower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), ((RelicModel)this).Owner.Creature, ((RelicModel)this).DynamicVars["Amount"].BaseValue, ((RelicModel)this).Owner.Creature, (CardModel?)null, silent: false);
		}
	}
}
