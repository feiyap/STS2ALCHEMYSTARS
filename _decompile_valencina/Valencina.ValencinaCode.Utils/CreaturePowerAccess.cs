using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Character;

namespace Valencina.ValencinaCode.Utils;

public static class CreaturePowerAccess
{
	public static bool IsValencina(Creature? creature)
	{
		object obj;
		if (creature == null)
		{
			obj = null;
		}
		else
		{
			Player player = creature.Player;
			obj = ((player != null) ? player.Character : null);
		}
		return obj is Valencina.ValencinaCode.Character.Valencina;
	}

	public static Player? GetPlayer(Creature? creature)
	{
		if (creature == null)
		{
			return null;
		}
		return creature.Player;
	}

	public static IEnumerable<PowerModel> Enumerate(Creature? creature)
	{
		IEnumerable<PowerModel> enumerable = ((creature != null) ? creature.Powers : null);
		return enumerable ?? Enumerable.Empty<PowerModel>();
	}

	public static TPower? Find<TPower>(Creature? creature) where TPower : class
	{
		return Enumerate(creature).OfType<TPower>().FirstOrDefault();
	}
}
