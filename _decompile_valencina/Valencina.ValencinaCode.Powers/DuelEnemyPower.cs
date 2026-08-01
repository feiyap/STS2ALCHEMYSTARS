using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Valencina.ValencinaCode.Compat;

namespace Valencina.ValencinaCode.Powers;

public sealed class DuelEnemyPower : ValencinaPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)2;

	protected override bool IsVisibleInternal => false;

	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Owner.Side == side && participants.Contains(((PowerModel)this).Owner) && ((PowerModel)this).Owner.IsAlive)
		{
			await CompatPowerCmd.Apply<StrengthPower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), ((PowerModel)this).Owner, 3m, ((PowerModel)this).Owner, (CardModel?)null, silent: false);
		}
	}
}
