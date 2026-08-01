using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Valencina.ValencinaCode.Powers;

public sealed class RodyaGuardPower : ValencinaPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)2;

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target != ((PowerModel)this).Owner || !AllEnemyAlliesAlive())
		{
			return 1m;
		}
		return 0.5m;
	}

	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Owner.Side == side && !AllEnemyAlliesAlive())
		{
			await PowerCmd.Remove((PowerModel)(object)this);
		}
	}

	private bool AllEnemyAlliesAlive()
	{
		ICombatState combatState = ((PowerModel)this).Owner.CombatState;
		List<Creature> list = ((combatState != null) ? (from ally in ((IEnumerable<Creature>)combatState.Enemies).Select((Func<Creature, Creature>)delegate(Creature enemy)
			{
				if (enemy == null)
				{
					return (Creature)null;
				}
				MonsterModel monster = enemy.Monster;
				return (monster == null) ? null : monster.Creature;
			})
			where ally != null && ally != ((PowerModel)this).Owner
			select ally).Cast<Creature>().ToList() : null) ?? new List<Creature>();
		if (list.Count > 0)
		{
			return list.All((Creature ally) => ally.IsAlive);
		}
		return false;
	}
}
