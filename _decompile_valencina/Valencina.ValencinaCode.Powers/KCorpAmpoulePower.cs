using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Valencina.ValencinaCode.Powers;

public sealed class KCorpAmpoulePower : ValencinaPower
{
	private bool _pendingFullHeal;

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override decimal ModifyHpLostAfterOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target != ((PowerModel)this).Owner || ((PowerModel)this).Amount <= 0 || amount <= 0m || ((PowerModel)this).Owner.CurrentHp <= 0)
		{
			return amount;
		}
		if (amount < (decimal)((PowerModel)this).Owner.CurrentHp)
		{
			return amount;
		}
		_pendingFullHeal = true;
		((PowerModel)this).Flash();
		return 0m;
	}

	public override async Task AfterModifyingHpLostAfterOsty()
	{
		if (_pendingFullHeal && ((PowerModel)this).Amount > 0)
		{
			_pendingFullHeal = false;
			await TriggerFullHeal();
		}
	}

	public override bool ShouldDie(Creature creature)
	{
		if (creature != ((PowerModel)this).Owner || ((PowerModel)this).Amount <= 0)
		{
			return true;
		}
		_pendingFullHeal = true;
		return false;
	}

	public override bool ShouldDieLate(Creature creature)
	{
		if (creature != ((PowerModel)this).Owner || ((PowerModel)this).Amount <= 0)
		{
			return true;
		}
		_pendingFullHeal = true;
		return false;
	}

	public override async Task AfterPreventingDeath(Creature creature)
	{
		if (creature == ((PowerModel)this).Owner && ((PowerModel)this).Amount > 0)
		{
			_pendingFullHeal = false;
			await TriggerFullHeal();
		}
	}

	public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		if (wasRemovalPrevented && creature == ((PowerModel)this).Owner && ((PowerModel)this).Amount > 0)
		{
			_pendingFullHeal = false;
			await TriggerFullHeal();
		}
	}

	public override bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature)
	{
		if (creature != ((PowerModel)this).Owner || ((PowerModel)this).Amount <= 0)
		{
			return true;
		}
		return false;
	}

	public override bool ShouldPowerBeRemovedAfterOwnerDeath()
	{
		return false;
	}

	public override bool ShouldStopCombatFromEnding()
	{
		return false;
	}

	private async Task TriggerFullHeal()
	{
		((PowerModel)this).Flash();
		await CreatureCmd.SetCurrentHp(((PowerModel)this).Owner, (decimal)((PowerModel)this).Owner.MaxHp);
		((PowerModel)this).SetAmount(((PowerModel)this).Amount - 1, false);
		if (((PowerModel)this).Amount <= 0)
		{
			await PowerCmd.Remove((PowerModel)(object)this);
		}
	}
}
