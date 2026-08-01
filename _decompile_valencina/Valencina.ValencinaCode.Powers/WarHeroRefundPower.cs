using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Powers;

public sealed class WarHeroRefundPower : ValencinaPower, IAmmoConsumedListener
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)0;

	public override bool AllowNegative => false;

	public async Task OnAmmoConsumedAsync(int consumed, int requested, Creature owner, Player? player, CardModel? sourceCard)
	{
		if (((PowerModel)this).Owner != null && owner == ((PowerModel)this).Owner && consumed > 0)
		{
			int num = consumed / 2;
			if (num > 0)
			{
				((PowerModel)this).Flash();
				await AmmoSystem.AddAmmoAsync(((PowerModel)this).Owner, num, sourceCard);
			}
		}
	}
}
