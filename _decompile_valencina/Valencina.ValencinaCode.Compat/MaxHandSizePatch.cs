using MegaCrit.Sts2.Core.Entities.Players;
using STS2RitsuLib.Combat.HandSize;

namespace Valencina.ValencinaCode.Compat;

public static class MaxHandSizePatch
{
	public static int GetMaxHandSize(Player player, int baseAmount = 10)
	{
		return MaxHandSizeCalculator.ApplyHookListenerModifiers(player, baseAmount);
	}
}
