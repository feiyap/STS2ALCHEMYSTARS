using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Systems;

public interface IAmmoReloadedListener
{
	Task OnAmmoReloadedAsync(int added, Creature owner, Player? player, CardModel? sourceCard);
}
