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

public sealed class GregorMercyPower : ValencinaPower
{
	private int _triggersThisTurn;

	private bool _isTriggering;

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)2;

	public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Owner.Side == side)
		{
			_triggersThisTurn = 0;
		}
		return Task.CompletedTask;
	}

	public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (_isTriggering || _triggersThisTurn >= 2 || !((PowerModel)this).Owner.IsAlive || !target.IsPlayer || result.UnblockedDamage <= 0)
		{
			return;
		}
		_triggersThisTurn++;
		_isTriggering = true;
		try
		{
			((PowerModel)this).Flash();
			await CreatureCmd.Damage(choiceContext, target, 2m, (ValueProp)8, ((PowerModel)this).Owner, (CardModel)null);
		}
		finally
		{
			_isTriggering = false;
		}
	}
}
