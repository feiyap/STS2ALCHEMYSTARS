using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Powers;

public interface IBreathingMethodConsumedListener
{
	Task OnBreathingMethodConsumedAsync(PlayerChoiceContext choiceContext, int consumed, Creature owner, CardModel? sourceCard);
}
