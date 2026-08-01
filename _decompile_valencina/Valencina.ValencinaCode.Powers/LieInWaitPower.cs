using MegaCrit.Sts2.Core.Entities.Powers;

namespace Valencina.ValencinaCode.Powers;

public sealed class LieInWaitPower : ValencinaPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)0;

	public override bool AllowNegative => false;
}
