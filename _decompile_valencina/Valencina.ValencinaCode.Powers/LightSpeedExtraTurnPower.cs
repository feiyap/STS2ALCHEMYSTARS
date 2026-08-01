using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Powers;

public sealed class LightSpeedExtraTurnPower : ValencinaPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	protected override bool IsVisibleInternal => false;

	public override bool ShouldTakeExtraTurn(Player player)
	{
		if (((PowerModel)this).Owner != null && player.Creature == ((PowerModel)this).Owner)
		{
			return ((PowerModel)this).Amount > 0;
		}
		return false;
	}

	public override async Task AfterTakingExtraTurn(Player player)
	{
		if (((PowerModel)this).Owner != null && player.Creature == ((PowerModel)this).Owner)
		{
			await PowerCmd.Remove((PowerModel)(object)this);
		}
	}
}
