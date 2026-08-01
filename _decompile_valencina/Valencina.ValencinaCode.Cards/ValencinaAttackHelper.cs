using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Cards;

public static class ValencinaAttackHelper
{
	public static Task RunAsync(ValencinaCard card, Func<Task> attackAction)
	{
		bool preserveBreathingMethod = card is IInstantAttackCard;
		return RunAsync(card, preserveBreathingMethod, attackAction);
	}

	public static async Task RunAsync(ValencinaCard card, bool preserveBreathingMethod, Func<Task> attackAction)
	{
		Player owner = ((CardModel)card).Owner;
		Creature val = ((owner != null) ? owner.Creature : null);
		if (val == null)
		{
			await attackAction();
		}
		else
		{
			await ValencinaAttackScope.RunAsync(val, preserveBreathingMethod, attackAction);
		}
	}
}
