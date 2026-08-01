using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Valencina.ValencinaCode.Powers;

public sealed class AttackPestPower : ValencinaPower
{
	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)1;

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Owner.Side == side && ((PowerModel)this).Amount > 0 && ((PowerModel)this).Owner.MaxHp > 0 && ((PowerModel)this).Owner.IsAlive)
		{
			int num = Math.Max(1, (int)Math.Ceiling((decimal)((PowerModel)this).Owner.MaxHp * ((decimal)((PowerModel)this).Amount / 100m)));
			await CreatureCmd.Damage(choiceContext, ((PowerModel)this).Owner, (decimal)num, (ValueProp)6, (Creature)null, (CardModel)null);
		}
	}
}
