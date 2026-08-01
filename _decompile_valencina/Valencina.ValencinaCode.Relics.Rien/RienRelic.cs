using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Runs;

namespace Valencina.ValencinaCode.Relics.Rien;

public abstract class RienRelic : ValencinaRelic
{
	public override RelicRarity Rarity => (RelicRarity)7;

	public override bool IsAllowed(IRunState runState)
	{
		return false;
	}
}
