using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Cards;

internal static class ValencinaCardStableKeys
{
	internal static string Creature(Creature creature)
	{
		object obj = creature.CombatId?.ToString("D10");
		if (obj == null)
		{
			Player player = creature.Player;
			obj = ((player != null) ? player.NetId.ToString() : null);
			if (obj == null)
			{
				MonsterModel monster = creature.Monster;
				obj = ((monster != null) ? ((AbstractModel)monster).Id.Entry : null) ?? creature.Name ?? ((object)creature).GetHashCode().ToString("D10");
			}
		}
		return (string)obj;
	}
}
