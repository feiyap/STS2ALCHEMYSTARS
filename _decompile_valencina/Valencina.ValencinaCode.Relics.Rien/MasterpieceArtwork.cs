using System.Linq;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Valencina.ValencinaCode.Utils;

namespace Valencina.ValencinaCode.Relics.Rien;

public sealed class MasterpieceArtwork : RienRelic
{
	private const int RequiredDebuffTypes = 3;

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		Player owner = ((RelicModel)this).Owner;
		if (((owner != null) ? owner.Creature : null) == null || target != ((RelicModel)this).Owner.Creature || dealer == null || dealer == ((RelicModel)this).Owner.Creature)
		{
			return 1m;
		}
		if (dealer.Side == ((RelicModel)this).Owner.Creature.Side)
		{
			return 1m;
		}
		if ((from power in CreaturePowerAccess.Enumerate(dealer)
			where (int)power.Type == 2
			select ((object)power).GetType()).Distinct().Count() <= 3)
		{
			return 1m;
		}
		((RelicModel)this).Flash();
		return 0.5m;
	}
}
