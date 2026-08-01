using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Compat;
using Valencina.ValencinaCode.Patches;
using Valencina.ValencinaCode.Settings;

namespace Valencina.ValencinaCode.Relics.Rien;

public sealed class Moth : RienRelic
{
	private const decimal DebuffStacks = 2m;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[3]
	{
		(DynamicVar)new PowerVar<VulnerablePower>("Vulnerable", 2m),
		(DynamicVar)new PowerVar<WeakPower>("Weak", 2m),
		(DynamicVar)new PowerVar<FrailPower>("Frail", 2m)
	});

	public override bool HasUponPickupEffect => true;

	public override bool IsAllowed(IRunState runState)
	{
		return false;
	}

	public override async Task AfterObtained()
	{
		if (ValencinaModConfig.EnableKaiserContent)
		{
			await UngezieferKaiserFinalBossController.TryApplyAndRegenerateCurrentMap(((RelicModel)this).Owner.RunState);
		}
	}

	public override async Task BeforeCombatStart()
	{
		Player owner = ((RelicModel)this).Owner;
		if (((owner != null) ? owner.Creature : null) != null && ValencinaWarDifficulty.IsActive(((RelicModel)this).Owner.RunState) && ((RelicModel)this).Owner.GetRelic<Moth>() == this)
		{
			((RelicModel)this).Flash();
			BlockingPlayerChoiceContext choiceContext = new BlockingPlayerChoiceContext();
			switch (((RelicModel)this).Owner.RunState.Rng.Niche.NextInt(3))
			{
			case 0:
				await CompatPowerCmd.Apply<VulnerablePower>((PlayerChoiceContext)(object)choiceContext, ((RelicModel)this).Owner.Creature, 2m, ((RelicModel)this).Owner.Creature, (CardModel?)null, silent: false);
				break;
			case 1:
				await CompatPowerCmd.Apply<WeakPower>((PlayerChoiceContext)(object)choiceContext, ((RelicModel)this).Owner.Creature, 2m, ((RelicModel)this).Owner.Creature, (CardModel?)null, silent: false);
				break;
			default:
				await CompatPowerCmd.Apply<FrailPower>((PlayerChoiceContext)(object)choiceContext, ((RelicModel)this).Owner.Creature, 2m, ((RelicModel)this).Owner.Creature, (CardModel?)null, silent: false);
				break;
			}
		}
	}
}
