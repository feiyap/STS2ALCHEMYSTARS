using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Relics.Rien;

public sealed class RevengeLedgerAppendix : RienRelic
{
	public const decimal CounterDamageMultiplier = 2m;

	public override bool HasUponPickupEffect => true;

	public override Task AfterObtained()
	{
		Player owner = ((RelicModel)this).Owner;
		((owner != null) ? owner.GetRelic<BernoullitMemory>() : null)?.UpgradeAllCounterStyles();
		return Task.CompletedTask;
	}
}
