using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using Valencina.ValencinaCode.Monsters;
using Valencina.ValencinaCode.Powers;

namespace Valencina.ValencinaCode.Patches;

[HarmonyPatch(typeof(CreatureCmd), "LoseBlock")]
internal static class UngezieferKaiserForcedBlockClearPatch
{
	private static bool Prefix(Creature creature, decimal amount, ref Task __result)
	{
		if (!(((creature != null) ? creature.Monster : null) is UngezieferKaiser) || amount <= 0m || creature.Block <= 0 || amount < (decimal)creature.Block)
		{
			return true;
		}
		if (creature.GetPower<KaiserImperialMandatePower>() == null)
		{
			return true;
		}
		__result = Task.CompletedTask;
		return false;
	}
}
