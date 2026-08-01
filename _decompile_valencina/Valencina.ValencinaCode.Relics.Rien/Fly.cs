using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Valencina.ValencinaCode.Patches;
using Valencina.ValencinaCode.Settings;

namespace Valencina.ValencinaCode.Relics.Rien;

public sealed class Fly : RienRelic
{
	public const int AdditionalAmbushChancePercent = 10;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Percent", 10m));

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
}
