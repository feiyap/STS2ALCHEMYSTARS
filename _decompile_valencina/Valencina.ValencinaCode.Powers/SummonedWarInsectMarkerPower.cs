using MegaCrit.Sts2.Core.Entities.Powers;

namespace Valencina.ValencinaCode.Powers;

public sealed class SummonedWarInsectMarkerPower : ValencinaPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)2;

	protected override bool IsVisibleInternal => false;
}
