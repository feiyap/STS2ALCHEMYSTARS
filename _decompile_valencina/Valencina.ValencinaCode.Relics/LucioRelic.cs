using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Runs;

namespace Valencina.ValencinaCode.Relics;

public sealed class LucioRelic : ValencinaRelic
{
	public override RelicRarity Rarity => (RelicRarity)6;

	public override bool IsAllowed(IRunState runState)
	{
		return false;
	}
}
