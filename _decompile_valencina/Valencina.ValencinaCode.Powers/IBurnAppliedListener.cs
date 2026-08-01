using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Powers;

public interface IBurnAppliedListener
{
	Task OnBurnAppliedAsync(PlayerChoiceContext choiceContext, Creature target, int amount, CardModel? sourceCard);
}
