using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Compat;

public static class CompatPowerCmd
{
	public static async Task<TPower?> Apply<TPower>(PlayerChoiceContext choiceContext, Creature target, decimal amount, Creature? applier, CardModel? cardSource, bool silent = false) where TPower : PowerModel
	{
		return (await PowerCmd.Apply<TPower>(choiceContext, (IEnumerable<Creature>)(object)new Creature[1] { target }, amount, applier, cardSource, silent)).FirstOrDefault();
	}

	public static Task ModifyAmount(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource, bool silent = false)
	{
		return PowerCmd.ModifyAmount(choiceContext, power, amount, applier, cardSource, silent);
	}
}
