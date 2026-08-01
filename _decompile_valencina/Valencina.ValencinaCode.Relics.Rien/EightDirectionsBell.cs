using System.Collections.Generic;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Relics.Rien;

public sealed class EightDirectionsBell : RienRelic
{
	public const int BreathingMethodGain = 2;

	protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[1] { (DynamicVar)new PowerVar<BreathingMethodPower>("Amount", 2m) };
}
