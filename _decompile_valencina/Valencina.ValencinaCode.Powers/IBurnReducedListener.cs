using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Valencina.ValencinaCode.Powers;

public interface IBurnReducedListener
{
	Task OnEnemyBurnReducedAsync(PlayerChoiceContext choiceContext, Creature target, int reducedAmount);
}
