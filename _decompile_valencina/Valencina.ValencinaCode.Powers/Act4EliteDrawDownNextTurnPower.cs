using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace Valencina.ValencinaCode.Powers;

public sealed class Act4EliteDrawDownNextTurnPower : ValencinaPower
{
	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)1;

	public override decimal ModifyHandDraw(Player player, decimal count)
	{
		if (player != ((PowerModel)this).Owner.Player || ((PowerModel)this).AmountOnTurnStart == 0)
		{
			return count;
		}
		((PowerModel)this).Flash();
		return count - (decimal)((PowerModel)this).Amount;
	}

	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Owner.Side == side && ((PowerModel)this).AmountOnTurnStart != 0)
		{
			await PowerCmd.Remove((PowerModel)(object)this);
		}
	}
}
