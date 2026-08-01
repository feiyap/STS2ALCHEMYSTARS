using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Valencina.ValencinaCode.Systems;

namespace Valencina.ValencinaCode.Powers;

public sealed class GetLostCounterPower : ValencinaPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool AllowNegative => false;

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		Creature owner = ((PowerModel)this).Owner;
		if (owner != null && side == owner.Side)
		{
			int num = AmmoSystem.MaxAmmoFor(owner) / 2;
			if (num > 0)
			{
				await AmmoSystem.AddAmmoAsync(owner, num, null, choiceContext);
			}
			await PowerCmd.Remove((PowerModel)(object)this);
		}
	}
}
