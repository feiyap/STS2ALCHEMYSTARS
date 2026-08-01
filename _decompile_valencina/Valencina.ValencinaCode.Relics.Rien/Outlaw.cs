using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Relics.Rien;

public sealed class Outlaw : RienRelic
{
	private const string MaxHpLossKey = "MaxHpLoss";

	private const string DamagePercentKey = "DamagePercent";

	public override bool HasUponPickupEffect => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[2]
	{
		new DynamicVar("MaxHpLoss", 10m),
		new DynamicVar("DamagePercent", 2m)
	};

	public override async Task AfterObtained()
	{
		await CreatureCmd.LoseMaxHp((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), ((RelicModel)this).Owner.Creature, ((RelicModel)this).DynamicVars["MaxHpLoss"].BaseValue, false);
	}

	public override async Task BeforeCombatStart()
	{
		Player owner = ((RelicModel)this).Owner;
		if (((owner != null) ? owner.Creature : null) != null)
		{
			await CompatPowerCmd.Apply<OutlawPower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), ((RelicModel)this).Owner.Creature, 1m, ((RelicModel)this).Owner.Creature, (CardModel?)null, silent: false);
		}
	}
}
