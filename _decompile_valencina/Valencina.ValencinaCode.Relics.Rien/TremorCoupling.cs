using System.Collections.Generic;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Valencina.ValencinaCode.Relics.Rien;

public sealed class TremorCoupling : RienRelic
{
	public const int TotalTremor = 10;

	protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[1]
	{
		new DynamicVar("Amount", 10m)
	};
}
