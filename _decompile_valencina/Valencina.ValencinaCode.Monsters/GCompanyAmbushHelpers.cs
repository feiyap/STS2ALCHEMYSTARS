using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content;
using Valencina.ValencinaCode.Compat;

namespace Valencina.ValencinaCode.Monsters;

internal static class GCompanyAmbushHelpers
{
	internal static Task<AttackCommand> Attack(ModMonsterTemplate monster, decimal damage, int hits)
	{
		return Act4EliteHelpers.ExecuteMonsterAttack(monster, damage, hits);
	}

	internal static async Task ApplyToPlayers<TPower>(ICombatState? combatState, Creature applier, decimal amount) where TPower : PowerModel
	{
		BlockingPlayerChoiceContext context = new BlockingPlayerChoiceContext();
		foreach (Creature item in Act4EliteHelpers.LivingPlayers(combatState))
		{
			await CompatPowerCmd.Apply<TPower>((PlayerChoiceContext)(object)context, item, amount, applier, null);
		}
	}
}
