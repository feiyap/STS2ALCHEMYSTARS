using MegaCrit.Sts2.Core.Entities.Powers;

namespace Valencina.ValencinaCode.Powers;

public sealed class AmmoCapacityPower : ValencinaPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;
}
