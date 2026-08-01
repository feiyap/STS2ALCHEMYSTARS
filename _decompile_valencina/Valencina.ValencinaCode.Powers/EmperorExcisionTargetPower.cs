using MegaCrit.Sts2.Core.Entities.Powers;

namespace Valencina.ValencinaCode.Powers;

public sealed class EmperorExcisionTargetPower : ValencinaPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	protected override bool IsVisibleInternal => false;
}
